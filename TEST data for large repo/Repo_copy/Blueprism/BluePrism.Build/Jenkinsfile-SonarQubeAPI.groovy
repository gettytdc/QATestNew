/*
  Jenkins Groovy pipeline script to run the SonarQube Analysis job on the specified project/branch
  
  The calling Jenkins job must define the following parameters:
    BRANCH              - The git branch to be built/analysed
    SonarQubeProjectKey - The project-key passed to SonarQube to select the project to be analysed
*/
node("build") {
    ansiColor('xterm') {
        stage('Prepare') {
            echo "Setting env vars"
            // Slack channel to receive messages about this job
            env.slackChannel            = "#development"
            // the git repo url for this project
            env.gitUrl                  = "git@dev.blueprism.com:development/automate.git"
            // SonarQube server instance from Jenkins (sets server URL & Token)
            env.sonarQubeInstanceName   = "BP-SQ"
            // name of MSBuild for SonarQube tool, configured in Jenkins' Global Tools
            env.sonarGlobalToolName     = "SonarScanner-Latest-NETFramework"
            // the url to this project's SonarQube dashboard
            env.sonarQubeDashboardUrl   = "https://sonarqube.blueprism.co.uk/dashboard?id=${params.SonarQubeProjectKey}"
            
            echo "Cleaning workspace"
            //cleanWs deleteDirs: true, patterns: [[pattern: '.git/', type: 'EXCLUDE']]
            cleanWs deleteDirs: true
            buildName '#${BUILD_NUMBER}-${BRANCH}'
        }
        stage('Git') {
            echo "Cloning ${params.BRANCH}"
            checkout([$class: 'GitSCM', branches: [[name: '**/'+ params.BRANCH]],
                extensions: [[$class: 'CloneOption', timeout: 3600, noTags: true], 
                            [$class: 'CleanBeforeCheckout']],
                gitTool: 'Default', 
                userRemoteConfigs: [[credentialsId: 'gitlab-deploy', url: env.gitUrl]] ])
        }
        stage('BuildAndSQScan') {
            // get the path to the SonarQube installation - using the name of the SQ installation from Jenkins' Global Tool Configuration
            def sqScannerHome = tool "${sonarGlobalToolName}"
            try {
                withSonarQubeEnv("${sonarQubeInstanceName}") {
                    bat label: "SonarStart", script: "${sqScannerHome}\\SonarScanner.MSBuild.exe begin /k:${params.SonarQubeProjectKey} /v:#${BUILD_ID} /d:sonar.projectBaseDir=${WORKSPACE}/BluePrism.Api/ /d:sonar.cs.dotcover.reportsPaths=\"${WORKSPACE}\\Output\\dotCover.Output.html\""

                    try {
                        bat label: 'NugetRestore', script: 'IF EXIST BluePrism.Build\\nugetRestore.bat ( CD BluePrism.Build && nugetRestore.bat ) ELSE ( @echo Skipping NuGet restore - no batch  file )'
                        
                        bat label: "MSBuildBP", script: "\"${tool 'VS2017'}\" BluePrism.Build\\build.targets /p:Configuration=Debug /p:BluePrismPlatforms=x64 /p:UnitTestCoverageEnabled=true /maxcpucount /nodeReuse:false"

                        bat label: "MSBuildAPI", script: "\"${tool 'VS2017'}\" BluePrism.Build\\build-api.targets /p:Configuration=Release /nodeReuse:false"
                    }
                    catch (Exception e) {
                        env.msg = "Job failed at the Build stage for ${JOB_BASE_NAME}:\r\n```" + e.toString() +"```"
                        currentBuild.result = "FAILURE"
                    }

                    bat label: "SonarEnd", script: "${sqScannerHome}\\SonarScanner.MSBuild.exe end"
                }
            }
            catch (Exception e) {
                env.msg = "Job failed at the SonarQube analysis stage for ${JOB_BASE_NAME}:\r\n```" + e.toString() +"```"
                currentBuild.result = "FAILURE"
            }
        }
        stage("QualityGate") {
            echo "QualityGate: currentBuild.currentResult=${currentBuild.currentResult}"
            if(currentBuild.currentResult != "FAILURE") {
                // wait for the results callback from QualityGate
                try {
                    timeout(time: 2, unit: 'HOURS') {
                        qg = waitForQualityGate()
                        echo "QualityGate scan returned with status [${qg.status}]"
                        if (qg.status == 'OK') {
                            env.msg = "Quality Gate passed for ${JOB_BASE_NAME}!"
                            currentBuild.result = "SUCCESS"
                        }
                        else { // WARN or ERROR
                            env.msg = "Quality Gate did not pass for ${JOB_BASE_NAME}."
                            currentBuild.result = "UNSTABLE"
                        }
                    }
                }
                catch (org.jenkinsci.plugins.workflow.steps.FlowInterruptedException e) {
                    env.msg = "Quality Gate timed out for ${JOB_BASE_NAME}."
                    currentBuild.result = "FAILURE"
                }
                catch (Exception e) {
                    env.msg = "Quality Gate hit an unexpected error for ${JOB_BASE_NAME}.\r\n```" + e.toString() +"```"
                    currentBuild.result = "FAILURE"
                }
            }
            else {
                echo "Not bothering to waitForQualityGate because the build/scan stage failed."
            }
        }
        stage('Notify') {
            echo env.msg
            env.slackMsg = "${env.msg} \r\n<${env.BUILD_URL} | Jenkins Build> | <${env.sonarQubeDashboardUrl} | SonarQube Project Dashboard>"
            switch(currentBuild.currentResult) {
                case "FAILURE":
                    manager.addErrorBadge(env.msg)
                    slackSend channel: "${env.slackChannel}", color: 'danger', message: ":boom: ${env.slackMsg}"
                    error env.msg
                    break
                case "UNSTABLE":
                    manager.addWarningBadge(env.msg)
                    slackSend channel: "${env.slackChannel}", color: 'warning', message: ":warning: ${env.slackMsg}"
                    unstable env.msg
                    break
                case "SUCCESS":
                    manager.addInfoBadge(env.msg)
                    slackSend channel: "${env.slackChannel}", color: 'good', message: ":thumbsup: ${env.slackMsg}"
                    break
                default:
                    echo "No notifications sent. The job ended in a weird state: ${currentBuild.currentResult}"
                    break
            }
        }
    }
}