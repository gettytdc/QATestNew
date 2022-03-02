#!/bin/bash

##
#
# Blue Prism Promotion / Metadata generation script
#
# A rather simple shell script (SSS) to move files about and
# generate JSON metadata.
#
##

# This file is kicked out by the build system and contains some of the 
# meat we need for the JSON file.
# Firstly, convert any Windows line endings to Unix
dos2unix ${WORKSPACE}/BluePrism.Build/Output/BuildData.properties

# Incorporate the variables in this file into the current env. We set
# allexport here so it doesn't matter that the properties file doesn't
# export any of its values.
set -o allexport
source ${WORKSPACE}/BluePrism.Build/Output/BuildData.properties
set +o allexport

# Set current working directory; we should be in the "bin" dir.
pushd ${WORKSPACE}/bin/

# Capture the current time in ISO 8601 format
CURRENT_TIMESTAMP=$(date +%FT%T%:z)

# Verify that the Enterprise MSI filenames exist
if [[ ! -f ${x86_FILENAME} || ! -f ${x64_FILENAME} ]]; then
    echo "One or more installers at ${x86_FILENAME} or ${x64_FILENAME}  did not exist or could not be found."
    exit 3
fi
# if BuildData.properties includes the trial, check file exists
if [[ ! -z ${TRIAL_x64_FILENAME} && ! -f ${TRIAL_x64_FILENAME} ]]; then
  echo "The trial installer at ${TRIAL_x64_FILENAME} did not exist or could not be found."
  exit 4
fi

# We firstly need to rename the Enterprise release to its proper name, using the 
# BuildData as a marker for the build files
x86_TARGET_FILENAME="BluePrism${FRIENDLY_VERSION}_x86.msi"
x64_TARGET_FILENAME="BluePrism${FRIENDLY_VERSION}_x64.msi"
# set target names for eval and learning
EVAL_x64_TARGET_FILENAME="BluePrismEvaluation${FRIENDLY_VERSION}.msi"
LEARN_x64_TARGET_FILENAME="BluePrismLearning${FRIENDLY_VERSION}.msi"

# Rename the files from their "Automate" name to their proper name
mv -v ${x86_FILENAME} ${x86_TARGET_FILENAME}
mv -v ${x64_FILENAME} ${x64_TARGET_FILENAME}
if [[ ! -z ${TRIAL_x64_FILENAME} ]]; then
    # the Evaluation and Learning editions are copies of the trial msi
    # copy the trial MSI and rename it so we have two outputs from the same file
    cp -v ${TRIAL_x64_FILENAME} ${EVAL_x64_TARGET_FILENAME}
    mv -v ${TRIAL_x64_FILENAME} ${LEARN_x64_TARGET_FILENAME}
fi

# Now generate some yummy JSON with the new filenames and the other
# metadata we already have.
JSON_TARGET_FILENAME="${FRIENDLY_VERSION}.json"

# Spit the content, substituted, out to the target file.
if [[ ! -z ${TRIAL_x64_FILENAME} ]]; then
cat <<EOF > ${JSON_TARGET_FILENAME}
[
  {
    "title": "Release ${FRIENDLY_VERSION}",
    "version": "${FRIENDLY_VERSION}",
    "installer": null,
    "x86": {
      "file": "${x86_TARGET_FILENAME}",
      "hash": "${x86_HASH}"
    },
    "x64": {
      "file": "${x64_TARGET_FILENAME}",
      "hash": "${x64_HASH}"
    },
    "updated_at": "${CURRENT_TIMESTAMP}",
    "created_at": "${CREATED_ON}",
    "type": "enterprise"
  },
  {
    "title": "Release ${FRIENDLY_VERSION} - Evaluation",
    "version": "${FRIENDLY_VERSION}",
    "installer": null,
    "x86": null,
    "x64": {
      "file": "${EVAL_x64_TARGET_FILENAME}",
      "hash": "${TRIAL_x64_HASH}"
    },
    "updated_at": "${CURRENT_TIMESTAMP}",
    "created_at": "${CREATED_ON}",
    "type": "evaluation"
  },
  {
    "title": "Release ${FRIENDLY_VERSION} - Learning",
    "version": "${FRIENDLY_VERSION}",
    "installer": null,
    "x86": null,
    "x64": {
      "file": "${LEARN_x64_TARGET_FILENAME}",
      "hash": "${TRIAL_x64_HASH}"
    },
    "updated_at": "${CURRENT_TIMESTAMP}",
    "created_at": "${CREATED_ON}",
    "type": "learning"
  }
]
EOF
else 
cat <<EOF > ${JSON_TARGET_FILENAME}
{
    "title": "Blue Prism ${FRIENDLY_VERSION}",
    "version": "${FRIENDLY_VERSION}",
    "x86": {
        "file": "${x86_TARGET_FILENAME}",
	"hash": "${x86_HASH}"
    },
    "x64": {
        "file": "${x64_TARGET_FILENAME}",
	"hash": "${x64_HASH}"
    },
    "updated_at": "${CURRENT_TIMESTAMP}",
    "created_at": "${CREATED_ON}"
}
EOF
fi

# Take us back to wherever we started, to avoid impacting anything else running in this shell.
popd

echo "Complete"
