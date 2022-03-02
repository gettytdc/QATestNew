using BluePrism.BPCoreLib.Collections;
using System;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;

namespace BluePrism.Images
{
    /// <summary>
    /// A class containing statically defined image lists.
    /// Note that image lists, being windows components, are not inherently
    /// thread-safe, so the thread which requests the image lists should be the UI
    /// thread which is handling the message pumping for the application.
    /// </summary>
    public static class ImageLists
    {
        #region - Public Extension Methods -

        /// <summary>
        /// Gets the image key within an image list of an index, or null if the
        /// index or image list does not exist.
        /// </summary>
        /// <param name="imgs">The imagelist from where to draw the key. Note: this
        /// can be null - if null is found, a null key is returned.</param>
        /// <param name="index">The index within the imagelist for which the image
        /// key is required.</param>
        /// <returns>The image key within the imagelist for the image at the
        /// specified index, or null if the imagelist was null or the index fell
        /// outside of its bounds.</returns>
        public static string GetKey(this ImageList imgs, int index)
        {
            if (imgs == null || index < 0 || index >= imgs.Images.Count)
                return null;
            return imgs.Images.Keys[index];
        }

        /// <summary>
        /// Gets the image index within the image list of the first occurrence of an
        /// image with a given image key.
        /// </summary>
        /// <param name="imgs">The ImageList to search for a key. Note: this can be
        /// null - if null is found, a value of -1 is returned.</param>
        /// <param name="key">The image key to search for within the image list.
        /// </param>
        /// <returns>The index at which the first occurrence of an image idenfitied
        /// by the given key can be found, or -1 if the imagelist was null or the key
        /// was not found in the imagelist's entries.</returns>
        public static int GetIndex(this ImageList imgs, string key)
        {
            if (imgs == null || key == null)
                return -1;
            return imgs.Images.IndexOfKey(key);
        }

        #endregion

        #region - Key Constants -

        /// <summary>
        /// The keys used in the image lists returned from this class
        /// </summary>
        public static class Keys
        {
            /// <summary>
            /// The key denoting a dotted line, for use in a treeview where 'no icon'
            /// should be displayed for an entry with icons set for other entries.
            /// </summary>
            public const string Dotted = "blank";

            public static class Robot
            {
                /// <summary>
                /// The 'active resource' image key
                /// </summary>
                /// <seealso cref="ImageLists.Robots_32x32"/>
                /// <seealso cref="ImageLists.Resources_16x16x"/>
                public const string Active = "active";

                /// <summary>
                /// The 'inactive resource' image key
                /// </summary>
                /// <seealso cref="ImageLists.Robots_32x32"/>
                /// <seealso cref="ImageLists.Resources_16x16x"/>
                public const string Inactive = "inactive";

                /// <summary>
                /// The 'pending resource' image key
                /// </summary>
                /// <seealso cref="ImageLists.Robots_32x32"/>
                /// <seealso cref="ImageLists.Resources_16x16x"/>
                public const string Pending = "pending";

            }

            public static class Component
            {
                /// <summary>
                /// Image Key for a package
                /// </summary>
                public const string Package = "package";
                /// <summary>
                /// Image Key for a release
                /// </summary>
                public const string Release = "release";
                /// <summary>
                /// Image Key for a release input into the environment
                /// </summary>
                public const string ReleaseIn = "releasein";
                /// <summary>
                /// Image Key for a release output from the environment
                /// </summary>
                public const string ReleaseOut = "releaseout";
                /// <summary>
                /// Image Key for a business object
                /// </summary>
                public const string Object = "object";
                /// <summary>
                /// Image Key for a process
                /// </summary>
                public const string Process = "process";
                /// <summary>
                /// Image Key for an application model
                /// </summary>
                public const string Model = "model";
                /// <summary>
                /// Image Key for a work queue
                /// </summary>
                public const string Queue = "work-queue";
                /// <summary>
                /// Image key for an active work queue
                /// </summary>
                public const string ActiveQueue = "active-queue";
                /// <summary>
                /// Image Key for a schedule
                /// </summary>
                public const string Schedule = "schedule";
                /// <summary>
                /// Image Key for a calendar
                /// </summary>
                public const string Calendar = "calendar";
                /// <summary>
                /// Image Key for a credential
                /// </summary>
                public const string Credential = "credential";
                /// <summary>
                /// Image Key for a web service
                /// </summary>
                public const string WebService = "web-service";
                /// <summary>
                /// Image Key for a web API
                /// </summary>
                public const string WebApi = "webapiservice";
                /// <summary>
                /// Image Key for an environment variable
                /// </summary>
                public const string EnvVar = "environment-variable";
                /// <summary>
                /// Image Key for a font
                /// </summary>
                public const string Font = "font";
                /// <summary>
                /// Image Key for a process group
                /// </summary>
                public const string ProcessGroup = "process-group";
                /// <summary>
                /// Image Key for a business object group
                /// </summary>
                public const string ObjectGroup = "object-group";
                /// <summary>
                /// Image Key for an application model group
                /// </summary>
                public const string ModelGroup = "model-group";
                /// <summary>
                /// Image Key for a work queue group
                /// </summary>
                public const string QueueGroup = "queue-group";
                /// <summary>
                /// Image key for a locked process
                /// </summary>
                public const string ProcessLocked = "process-locked";
                /// <summary>
                /// Image key for a locked object
                /// </summary>
                public const string ObjectLocked = "object-locked";
                /// <summary>
                /// Image key for a global process
                /// </summary>
                public const string ProcessGlobal = "process-global";
                /// <summary>
                /// Image key for a global object
                /// </summary>
                public const string ObjectGlobal = "object-global";
                /// <summary>
                /// Image key for a locked global process
                /// </summary>
                public const string ProcessGlobalLocked = "process-global-locked";
                /// <summary>
                /// Image key for a locked global object
                /// </summary>
                public const string ObjectGlobalLocked = "object-global-locked";
                /// <summary>
                /// Image Key for a open group
                /// </summary>
                public const string OpenGroup = "open-group";
                /// <summary>
                /// Image Key for a closed group
                /// </summary>
                public const string ClosedGroup = "closed-group";
                /// <summary>
                /// Image Key for an open group which is global
                /// </summary>
                public const string OpenGlobalGroup = "open-global-group";
                /// <summary>
                /// Image Key for a closed group which is global.
                /// </summary>
                public const string ClosedGlobalGroup = "closed-global-group";
                /// <summary>
                /// Image key for a robot
                /// </summary>
                public const string Robot = "robot";
                /// <summary>
                /// Image key for the root of a tile tree
                /// </summary>
                public const string TileTree = "tile-tree";
                /// <summary>
                /// Image key for a tile
                /// </summary>
                public const string Tile = "tile";
                /// <summary>
                /// Image Key for an active resource
                /// </summary>
                public const string ResourceActive = "resource-active";
                /// <summary>
                /// Image Key for an inactive resource
                /// </summary>
                public const string ResourceInactive = "resource-inactive";
                /// <summary>
                /// Image Key for an connecting resource
                /// </summary>
                public const string ResourceConnecting = "resource-connecting";
                /// <summary>
                /// Image Key for an resource in error state
                /// </summary>
                public const string ResourceError = "resource-error";
                /// <summary>
                /// Image Key for an resource that is in use.
                /// </summary>
                public const string ResourceInUse = "resource-inuse";
                /// <summary>
                /// Image Key for an offline resource
                /// </summary>
                public const string ResourceOffline = "resource-offline";
                /// <summary>
                /// Image key for a Login Agent resource
                /// </summary>
                public const string ResourceLoginAgent = "resource-loginagent";
                /// <summary>
                /// Image Key for a dashboard
                /// </summary>
                public const string Dashboard = "dashboard";
                /// <summary>
                /// Image Key for a data source
                /// </summary>
                public const string DataSource = "data-source";
                ///<summary>
                /// Image Key for a user 
                /// </summary>
                public const string User = "user";
                /// <summary>
                /// Image key for a user group
                /// </summary>
                public const string UserGroup = "user-group";
                /// <summary>
                /// Image key for a deleted user
                /// </summary>
                public const string User_Deleted = "user-deleted";
                /// <summary>
                /// Image key for a locked user
                /// </summary>
                public const string User_Locked = "user-locked";


                /// <summary>
                /// Image key for a resource pool
                /// </summary>
                public const string ResourcePool = "resource-pool";
                /// <summary>
                /// Image key for a resource pool
                /// </summary>
                public const string ResourcePoolMember = "resource-pool-member";
                /// <summary>
                /// Image key for a resource busy
                /// </summary>
                public const string ResourceBusy = "resource-busy";
                /// <summary>
                /// Image key for a resource in warning state
                /// </summary>
                public const string ResourceWarning = "resource-warning";

                public const string Skill = "skill";
                /// <summary>
                /// Image key for a resource that app server can't connect to
                /// </summary>
                public const string ResourceDisconnected = "resource-disconnected";


                public const string ServiceAccount = "service-account";
                public const string ServiceAccount_Disabled = "service-account-disabled";
            }

            public static class Tools
            {
                public const string Save = "save";
                public const string SaveAs = "saveas";
                public const string Print = "print";
                public const string Cut = "cut";
                public const string Copy = "copy";
                public const string Paste = "paste";
                public const string Delete = "delete";
                public const string Undo = "undo";
                public const string Redo = "redo";
                public const string Refresh = "refresh";
                public const string Bold = "bold";
                public const string Italic = "italic";
                public const string Underline = "underline";
                public const string Modeller = "modeller";
                public const string Play = "play";
                public const string Pause = "pause";
                public const string Step = "step";
                public const string StepOver = "stepover";
                public const string StepOut = "stepout";
                public const string Reset = "reset";
                public const string Validate = "validate";
                public const string Launch = "launch";
                public const string Breakpoint = "breakpoint";
                public const string Find = "find";
                public const string FindNext = "findnext";
                public const string Watch = "watch";
                public const string Statistics = "statistics";
                public const string FullScreen = "fullscreen";
                public const string Pan = "pan";
                public const string ExpressionEdit = "expressionedit";
                public const string DataItem = "dataitem";
                public const string LogViewer = "logviewer";
                public const string LogToggle = "logtoggle";
            }

            public static class Sysman
            {
                public const string Processes = "processes";
                public const string Objects = "objects";
                public const string Resources = "resources";
                public const string Workflow = "workflow";
                public const string Security = "security";
                public const string Audit = "audit";
                public const string System = "system";
                public const string Skills = "skills";
                public const string DocumentProcessing = "documentProcessing";
                public const string DataPipelines = "dataPipelines";
            }

            public static class ControlRoom
            {
                public const string SessionManagement = "session-management";
                public const string SessionFilter = "session-filter";
                public const string QueueManagement = "queue-management";
                public const string OpenGroup = Component.OpenGroup;
                public const string ClosedGroup = Component.ClosedGroup;
                public const string Queue = Component.Queue;
                public const string ActiveQueue = Component.ActiveQueue;
                public const string Scheduler = "scheduler";
                public const string Schedule = Component.Schedule;
                public const string ScheduleReport = "schedule-report";
                public const string ScheduleTimetable = "schedule-timetable";
                public const string SessionTask = "session-task";
                public const string RetiredSchedule = "schedule-retired";
                public const string RetiredSessionTask = "session-task-retired";
                public const string DataPipelinesOk = "dataPipelinesOk";
                public const string DataPipelinesError = "dataPipelinesError";
            }

            public static class Auth
            {
                public const string User = "user";
                public const string UserLocked = "userlocked";
                public const string Users = "users";
                public const string Process = "process";
                public const string Robot = "robot";
                public const string Process_Disabled = "process_disabled";
                public const string Robot_Disabled = "robot_disabled";
                public const string User_Disabled = "user_disabled";
                public const string Users_Disabled = "users_disabled";
                public const string ServiceAccount = "service_account";
                public const string ServiceAccount_Disabled = "service_account_disabled";
            }

            public static class Publishing
            {
                public const string Published = "published";
            }

            public static class Filtering
            {
                public const string Search = "search";
                public const string Clear = "clear";
                public const string Clear_Disabled = "clear_disabled";
            }
        }

        #endregion

        #region - Static Member Variables -

        /// <summary>
        /// The cached images representing a 'blank' entry in a treeview - in
        /// actuality, this is a dotted line so that it blends with the lines in a
        /// treeview.
        /// The map is keyed against the size of the dotted image.
        /// </summary>
        private static IDictionary<Size, Image> m_dottedImgs =
            GetSynced.IDictionary(new Dictionary<Size, Image>());

        // The 32x32 resources image list
        private static ImageList m_robots32;

        // The 16x16 resources image list
        private static ImageList m_robots16;

        // The 32x32 components image list
        private static ImageList m_components32;

        // The 16x16 components image list
        private static ImageList m_components16;

        // The 32x32 tools image list
        private static ImageList m_tools32;

        // The 16x16 tools image list
        private static ImageList m_tools16;

        // The 16x16 sysman image list
        private static ImageList m_sysman16;

        // The 16x16 control room image list
        private static ImageList m_controlroom16;

        // The 16x16 user image list
        private static ImageList m_auth16;

        // The 32x32 user image list
        private static ImageList m_auth32;

        // The 16x16 web image list
        private static ImageList m_web16;

        // The 32x32 web image list
        private static ImageList m_web32;

        // The 16x16 publishing image list
        private static ImageList m_publish16;

        // The 16x16 filtering image list
        private static ImageList m_filter16;

        #endregion

        #region - Create() methods -

        /// <summary>
        /// Creates the imagelist from the given dictionary. This will draw the size
        /// of the image list from the first image in the dictionary.
        /// </summary>
        /// <param name="map">The map of image keys to the images they represent
        /// which should be set in the generated image list</param>
        /// <returns>An imagelist containing the given images keyed on their supplied
        /// image keys</returns>
        /// <exception cref="ArgumentException">If the dictionary contained no
        /// entries or the first entry held a null image, meaning that the method
        /// could not determine the required size of the image list.</exception>
        private static ImageList Create(IDictionary<string, Image> map)
        {
            Image firstImg = map.Values.FirstOrDefault();
            if (firstImg != null)
                return Create(firstImg.Size, map);
            throw new ArgumentException("Cannot determine size of image list");
        }

        /// <summary>
        /// Creates the imagelist from the given dictionary
        /// </summary>
        /// <param name="sz">The size of the images in the list</param>
        /// <param name="map">The map of image keys to the images they represent
        /// which should be set in the generated image list</param>
        /// <returns>An imagelist containing the given images keyed on their supplied
        /// image keys</returns>
        private static ImageList Create(Size sz, IDictionary<string, Image> map)
        {
            var images = new ImageList() {
                ImageSize = sz, ColorDepth = ColorDepth.Depth32Bit
            };
            Image dotted;
            if (!m_dottedImgs.TryGetValue(sz, out dotted))
            {
                dotted = new Bitmap(sz.Width, sz.Height);
                using (Graphics g = Graphics.FromImage(dotted))
                {
                    g.Clear(Color.Transparent);
                    // We don't want it quite blank - with no icon there, winforms
                    // leaves a gap, so emulate the dotted line in the treeview.
                    using (Pen p = new Pen(SystemColors.GrayText))
                    {
                        p.DashStyle = DashStyle.Dot;
                        // Draw straight down the middle
                        g.DrawLine(p, 1, sz.Height / 2, sz.Width, sz.Height / 2);
                    }
                }
            }
            images.Images.Add(Keys.Dotted, dotted);
            foreach (KeyValuePair<string, Image> pair in map)
            {
                images.Images.Add(pair.Key, pair.Value);
            }
            return images;
        }

        #endregion

        #region - ImageList Properties -

        /// <summary>
        /// A 32x32 list of images which represent a resource in various states.
        /// </summary>
        /// <seealso cref="Keys.Robot.Active"/>
        /// <seealso cref="Keys.Robot.Inactive"/>
        /// <seealso cref="Keys.Robot.Pending"/>
        /// <seealso cref="Robots_16x16"/>
        public static ImageList Robots_32x32
        {
            get
            {
                if (m_robots32 == null)
                {
                    m_robots32 = Create(new clsOrderedDictionary<string, Image>() {
                        {Keys.Robot.Active, RobotImages.PC_32x32},
                        {Keys.Robot.Inactive, RobotImages.PC_Error_32x32_Disabled},
                        {Keys.Robot.Pending, RobotImages.PC_Warning_32x32}
                    });
                }
                return m_robots32;
            }
        }

        /// <summary>
        /// A 32x32 list of images which represent a resource in various states.
        /// </summary>
        /// <seealso cref="Keys.Robot.Active"/>
        /// <seealso cref="Keys.Robot.Inactive"/>
        /// <seealso cref="Keys.Robot.Pending"/>
        /// <seealso cref="Robots_32x32"/>
        public static ImageList Robots_16x16
        {
            get
            {
                if (m_robots16 == null)
                {
                    m_robots16 = Create(new clsOrderedDictionary<string, Image>() {
                        {Keys.Robot.Active, RobotImages.PC_16x16},
                        {Keys.Robot.Inactive, RobotImages.PC_Error_16x16_Disabled},
                        {Keys.Robot.Pending, RobotImages.PC_Warning_16x16}
                    });
                }
                return m_robots16;
            }
        }

        /// <summary>
        /// A 16x16 list of images which represent components within the environment
        /// </summary>
        public static ImageList Components_16x16
        {
            get
            {
                if (m_components16 == null)
                {
                    m_components16 = Create(new clsOrderedDictionary<string, Image>(){
                        {Keys.Component.Object, ComponentImages.Class_16x16},
                        {Keys.Component.Process, ComponentImages.Procedure_16x16},
                        {Keys.Component.Model, ComponentImages.Structure_16x16},
                        {Keys.Component.ProcessGroup, ComponentImages.Procedure_16x16},
                        {Keys.Component.ObjectGroup, ComponentImages.Class_16x16},
                        {Keys.Component.OpenGroup, ComponentImages.Folder_Open_16x16},
                        {Keys.Component.ClosedGroup, ComponentImages.Folder_Closed_16x16},
                        {Keys.Component.ModelGroup, ComponentImages.Structure_16x16},
                        {Keys.Component.QueueGroup, ComponentImages.Custom_Queue_16x16},
                        {Keys.Component.Queue, ComponentImages.Custom_Queue_16x16},
                        {Keys.Component.ActiveQueue, ComponentImages.Custom_Active_Queue_16x16},
                        {Keys.Component.Package, ComponentImages.Item_Details_16x16},
                        {Keys.Component.Release, ComponentImages.Item_16x16},
                        {Keys.Component.ReleaseIn, ComponentImages.Item_Add_16x16},
                        {Keys.Component.ReleaseOut, ComponentImages.Item_Tick_16x16},
                        {Keys.Component.Schedule, ComponentImages.Task_Schedule_16x16},
                        {Keys.Component.Calendar, ComponentImages.Date_Time_16x16},
                        {Keys.Component.Credential, ComponentImages.Key_16x16},
                        {Keys.Component.WebService, ComponentImages.Document_HTML_16x16},
                        {Keys.Component.WebApi, ComponentImages.Document_HTML_Purple_16x16},
                        {Keys.Component.EnvVar, ComponentImages.Field_16x16},
                        {Keys.Component.Font, ComponentImages.Control_Label_16x16},
                        {Keys.Component.ProcessLocked, ComponentImages.Procedure_Lock_16x16},
                        {Keys.Component.ObjectLocked, ComponentImages.Class_Lock_16x16},

                        {Keys.Component.ProcessGlobal, Combine(ComponentImages.Procedure_16x16, ComponentImages.Global_Group_16x16)},
                        {Keys.Component.ObjectGlobal, Combine(ComponentImages.Class_16x16, ComponentImages.Global_Group_16x16)},
                        {Keys.Component.ProcessGlobalLocked, Combine(ComponentImages.Procedure_Lock_16x16, ComponentImages.Global_Group_16x16) },
                        {Keys.Component.ObjectGlobalLocked, Combine(ComponentImages.Class_Lock_16x16, ComponentImages.Global_Group_16x16) },

                        {Keys.Component.OpenGlobalGroup, Combine(ComponentImages.Folder_Open_16x16, ComponentImages.Global_Group_16x16)},
                        {Keys.Component.ClosedGlobalGroup, Combine(ComponentImages.Folder_Closed_16x16, ComponentImages.Global_Group_16x16)},

                        {Keys.Component.Robot, RobotImages.PC_16x16},
                        {Keys.Component.TileTree, ToolImages.HTML_Inline_Frame_16x16},
                        {Keys.Component.Tile, ToolImages.Chart_Pie_16x16},
                        {Keys.Component.ResourceActive, RobotImages.Custom_Resource_Online_16x16},
                        {Keys.Component.ResourceInactive, RobotImages.Custom_Resource_Unavailable_16x16},
                        {Keys.Component.ResourceConnecting, RobotImages.Custom_Resource_Connecting_16x16},
                        {Keys.Component.ResourceError, RobotImages.Custom_Resource_Error_16x16},
                        {Keys.Component.ResourceInUse, RobotImages.PC_Lock_16x16},
                        {Keys.Component.ResourceOffline, RobotImages.Custom_Resource_Offline_16x16},
                        {Keys.Component.ResourceLoginAgent, RobotImages.Custom_Resource_LoginAgent_16x16},
                        {Keys.Component.Dashboard, ToolImages.Window_16x16},
                        {Keys.Component.DataSource, ToolImages.Database_16x16},
                        {Keys.Component.User, AuthImages.User_Blue_16x16},
                        {Keys.Component.UserGroup, AuthImages.Users_16x16},
                        {Keys.Component.User_Deleted, AuthImages.User_Blue_16x16_Disabled},
                        {Keys.Component.User_Locked, AuthImages.User_Blue_Lock_16x16},
                        {Keys.Component.ResourcePool, ToolImages.PC_Network_16x16},
                        {Keys.Component.ResourcePoolMember, ToolImages.PC_16x16},
                        {Keys.Component.ResourceBusy, RobotImages.Custom_Resource_Working_16x16},
                        {Keys.Component.ResourceWarning, RobotImages.Custom_Resource_Warning_16x16},
                        {Keys.Component.Skill, ComponentImages.Skill_16x16},
                        {Keys.Component.ServiceAccount, ComponentImages.Key_16x16},
                        {Keys.Component.ServiceAccount_Disabled, ComponentImages.Key_16x16_Disabled},
                        {Keys.Component.ResourceDisconnected, RobotImages.PC_Error_16x16}
                    });
                }
                return m_components16;
            }
        }

        /// <summary>
        /// A 32x32 list of images which represent one of various components
        /// </summary>
        public static ImageList Components_32x32
        {
            get
            {
                if (m_components32 == null)
                {
                    m_components32 = Create(new clsOrderedDictionary<string, Image>(){
                        {Keys.Component.Object, ComponentImages.Class_32x32},
                        {Keys.Component.Process, ComponentImages.Procedure_32x32},
                        {Keys.Component.Model, ComponentImages.Structure_32x32},
                        {Keys.Component.ProcessGroup, ComponentImages.Procedure_32x32},
                        {Keys.Component.ObjectGroup, ComponentImages.Class_32x32},
                        {Keys.Component.ModelGroup, ComponentImages.Structure_32x32},
                        {Keys.Component.QueueGroup, ComponentImages.Upload_32x32},
                        {Keys.Component.Queue, ComponentImages.Upload_32x32},
                        {Keys.Component.Package, ComponentImages.Item_Details_32x32},
                        {Keys.Component.Release, ComponentImages.Item_32x32},
                        {Keys.Component.ReleaseIn, ComponentImages.Item_Add_32x32},
                        {Keys.Component.ReleaseOut, ComponentImages.Item_Tick_32x32},
                        {Keys.Component.Schedule, ComponentImages.Task_Schedule_32x32},
                        {Keys.Component.Calendar, ComponentImages.Date_Time_32x32},
                        {Keys.Component.Credential, ComponentImages.Key_32x32},
                        {Keys.Component.WebService, ComponentImages.Document_HTML_32x32},
                        {Keys.Component.WebApi, ComponentImages.Document_HTML_Purple_32x32},
                        {Keys.Component.EnvVar, ComponentImages.Field_32x32},
                        {Keys.Component.Font, ComponentImages.Control_Label_32x32},
                        {Keys.Component.ProcessLocked, ComponentImages.Procedure_Lock_32x32},
                        {Keys.Component.ObjectLocked, ComponentImages.Class_Lock_32x32},

                        {Keys.Component.ProcessGlobal, Combine(ComponentImages.Procedure_32x32, ComponentImages.Global_Group_32x32) },
                        {Keys.Component.ObjectGlobal, Combine(ComponentImages.Class_32x32, ComponentImages.Global_Group_32x32)},
                        {Keys.Component.ProcessGlobalLocked, Combine(ComponentImages.Procedure_Lock_32x32, ComponentImages.Global_Group_32x32) },
                        {Keys.Component.ObjectGlobalLocked, Combine(ComponentImages.Class_Lock_32x32, ComponentImages.Global_Group_32x32) },

                        {Keys.Component.TileTree, ToolImages.HTML_Inline_Frame_32x32},
                        {Keys.Component.Tile, ToolImages.Chart_Pie_32x32},
                        {Keys.Component.ResourceActive, RobotImages.PC_32x32},
                        {Keys.Component.ResourceInactive, RobotImages.PC_Error_32x32_Disabled},
                        {Keys.Component.Dashboard, ToolImages.Window_32x32},
                        {Keys.Component.DataSource, ToolImages.Database_32x32},
                        {Keys.Component.User, AuthImages.User_Blue_32x32},
                        {Keys.Component.UserGroup, AuthImages.Users_32x32},
                        {Keys.Component.User_Deleted, AuthImages.User_Blue_32x32_Disabled},
                        {Keys.Component.User_Locked, AuthImages.User_Blue_Lock_32x32},
                    });
                }
                return m_components32;
            }
        }

        /// <summary>
        /// A 16x16 list of images which represent tool icons
        /// </summary>
        public static ImageList Tools_16x16
        {
            get
            {
                if (m_tools16 == null)
                {
                    m_tools16 = Create(new clsOrderedDictionary<string, Image>(){
                        {Keys.Tools.Bold, ToolImages.Bold_16x16},
                        {Keys.Tools.Italic, ToolImages.Italic_16x16},
                        {Keys.Tools.Underline, ToolImages.Underline_16x16},
                        {Keys.Tools.Save, ToolImages.Save_16x16},
                        {Keys.Tools.Undo, ToolImages.Undo_16x16},
                        {Keys.Tools.Redo, ToolImages.Redo_16x16},
                        {Keys.Tools.Refresh, ToolImages.Refresh_16x16},
                        {Keys.Tools.Step, ToolImages.Debug_Step_In_To_16x16},
                        {Keys.Tools.StepOut, ToolImages.Debug_Step_Out_16x16},
                        {Keys.Tools.StepOver, ToolImages.Debug_Step_Over_16x16},
                        {Keys.Tools.Breakpoint, ToolImages.Flag_Red_16x16},
                        {Keys.Tools.Cut, ToolImages.Cut_16x16},
                        {Keys.Tools.Copy, ToolImages.Copy_16x16},
                        {Keys.Tools.Paste, ToolImages.Paste_16x16},
                        {Keys.Tools.Delete, ToolImages.Delete_Red_16x16},
                        {Keys.Tools.DataItem, ComponentImages.Field_16x16},
                        {Keys.Tools.ExpressionEdit, ToolImages.Calculator_16x16},
                        {Keys.Tools.Find, ToolImages.Search_16x16},
                        {Keys.Tools.FindNext, ToolImages.Search_Next_16x16},
                        {Keys.Tools.FullScreen, ToolImages.Full_Screen_16x16},
                        {Keys.Tools.Launch, ToolImages.Launch_16x16},
                        {Keys.Tools.LogToggle, ToolImages.Script_View_16x16 },
                        {Keys.Tools.LogViewer, ToolImages. Script_Details_16x16},
                        {Keys.Tools.Modeller, ComponentImages.Structure_16x16},
                        {Keys.Tools.Pan, ToolImages.HTML_Inline_Frame_16x16 },
                        {Keys.Tools.Play, MediaImages.mm_Play_16x16},
                        {Keys.Tools.Pause, MediaImages.mm_Pause_16x16 },
                        {Keys.Tools.Print, ToolImages.Print_16x16 },
                        {Keys.Tools.Save, ToolImages.Save_16x16},
                        {Keys.Tools.SaveAs, ToolImages.Save_As_16x16},
                        {Keys.Tools.Statistics, ToolImages.Research_16x16},
                        {Keys.Tools.Validate, ToolImages.Notebook_Tick_16x16},
                        {Keys.Tools.Watch, ToolImages.Watch_16x16}
                    });
                }
                return m_tools16;
            }
        }

        /// <summary>
        /// A 32x32 list of images which represent tool icons
        /// </summary>
        public static ImageList Tools_32x32
        {
            get
            {
                if (m_tools32 == null)
                {
                    m_tools32 = Create(new clsOrderedDictionary<string, Image>(){
                        {Keys.Tools.Bold, ToolImages.Bold_32x32},
                        {Keys.Tools.Italic, ToolImages.Italic_32x32},
                        {Keys.Tools.Underline, ToolImages.Underline_32x32},
                        {Keys.Tools.Save, ToolImages.Save_32x32},
                        {Keys.Tools.Undo, ToolImages.Undo_32x32},
                        {Keys.Tools.Redo, ToolImages.Redo_32x32},
                        {Keys.Tools.Refresh, ToolImages.Refresh_32x32},
                        {Keys.Tools.Step, ToolImages.Debug_Step_In_To_32x32},
                        {Keys.Tools.StepOut, ToolImages.Debug_Step_Out_32x32},
                        {Keys.Tools.StepOver, ToolImages.Debug_Step_Over_32x32},
                        {Keys.Tools.Breakpoint, ToolImages.Flag_Red_32x32},
                        {Keys.Tools.Cut, ToolImages.Cut_32x32},
                        {Keys.Tools.Copy, ToolImages.Copy_32x32},
                        {Keys.Tools.Paste, ToolImages.Paste_32x32},
                        {Keys.Tools.Delete, ToolImages.Delete_Red_32x32},
                        {Keys.Tools.DataItem, ComponentImages.Field_32x32},
                        {Keys.Tools.ExpressionEdit, ToolImages.Calculator_32x32},
                        {Keys.Tools.Find, ToolImages.Search_32x32},
                        {Keys.Tools.FindNext, ToolImages.Search_Next_32x32},
                        {Keys.Tools.FullScreen, ToolImages.Full_Screen_32x32},
                        {Keys.Tools.Launch, ToolImages.Launch_32x32},
                        {Keys.Tools.LogToggle, ToolImages.Script_View_32x32 },
                        {Keys.Tools.LogViewer, ToolImages. Script_Details_32x32},
                        {Keys.Tools.Modeller, ComponentImages.Structure_32x32},
                        {Keys.Tools.Pan, ToolImages.HTML_Inline_Frame_32x32 },
                        {Keys.Tools.Play, MediaImages.mm_Play_32x32},
                        {Keys.Tools.Pause, MediaImages.mm_Pause_32x32 },
                        {Keys.Tools.Print, ToolImages.Print_32x32 },
                        {Keys.Tools.Save, ToolImages.Save_32x32},
                        {Keys.Tools.SaveAs, ToolImages.Save_As_32x32},
                        {Keys.Tools.Statistics, ToolImages.Research_32x32},
                        {Keys.Tools.Validate, ToolImages.Notebook_Tick_32x32},
                        {Keys.Tools.Watch, ToolImages.Watch_32x32}
                    });
                }
                return m_tools32;
            }
        }

        public static ImageList Sysman_16x16
        {
            get
            {
                if (m_sysman16 == null)
                {
                    m_sysman16 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Sysman.Processes, ComponentImages.Procedure_16x16},
                        {Keys.Sysman.Objects, ComponentImages.Class_16x16},
                        {Keys.Sysman.Resources, RobotImages.PC_16x16},
                        {Keys.Sysman.Workflow, ToolImages.Project_Project_Center_16x16},
                        {Keys.Sysman.Security, ComponentImages.Key_16x16},
                        {Keys.Sysman.Audit, ToolImages.Script_16x16},
                        {Keys.Sysman.System, ToolImages.Settings_16x16},
                        {Keys.Sysman.Skills, ComponentImages.Skill_16x16},
                        {Keys.Sysman.DocumentProcessing, ComponentImages.Control_Label_16x16},
                        {Keys.Sysman.DataPipelines, ToolImages.Database_Backup_16x16}
                    });
                }
                return m_sysman16;
            }
        }

        /// <summary>
        /// A 16x16 list of images for use in the control room treeview
        /// </summary>
        public static ImageList ControlRoom_16x16
        {
            get
            {
                if (m_controlroom16 == null)
                {
                    m_controlroom16 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.ControlRoom.SessionManagement, RobotImages.PC_Information_16x16 },
                        {Keys.ControlRoom.SessionFilter, ToolImages.Filter_16x16 },
                        {Keys.ControlRoom.QueueManagement, ToolImages.Project_Project_Center_16x16 },
                        {Keys.ControlRoom.OpenGroup, ComponentImages.Folder_Open_16x16 },
                        {Keys.ControlRoom.ClosedGroup, ComponentImages.Folder_Closed_16x16 },
                        {Keys.ControlRoom.Queue, ComponentImages.Custom_Queue_16x16 },
                        {Keys.ControlRoom.ActiveQueue, ComponentImages.Custom_Active_Queue_16x16 },
                        {Keys.ControlRoom.Scheduler, ComponentImages.Task_Schedule_16x16 },
                        {Keys.ControlRoom.Schedule, ToolImages.Scheduling_16x16 },
                        {Keys.ControlRoom.ScheduleReport, ToolImages.Script_16x16 },
                        {Keys.ControlRoom.ScheduleTimetable, ComponentImages.Calendar_16x16 },
                        {Keys.ControlRoom.SessionTask, ComponentImages.Custom_Bullet_Blue_16x16 },
                        {Keys.ControlRoom.RetiredSchedule, ToolImages.Scheduling_16x16_Disabled },
                        {Keys.ControlRoom.RetiredSessionTask, ComponentImages.Custom_Bullet_Blue_16x16_Disabled },
                        {Keys.ControlRoom.DataPipelinesOk, ToolImages.Database_Backup_16x16},
                        {Keys.ControlRoom.DataPipelinesError, ToolImages.Database_Error_16x16 }
                    });
                }
                return m_controlroom16;
            }
        }

        public static ImageList Auth_16x16
        {
            get
            {
                if (m_auth16 == null)
                {
                    m_auth16 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Auth.User, AuthImages.User_Blue_16x16},
                        {Keys.Auth.UserLocked, AuthImages.User_Blue_Lock_16x16},
                        {Keys.Auth.Users, AuthImages.Users_16x16},
                        {Keys.Auth.Process, ComponentImages.Procedure_16x16},
                        {Keys.Auth.Robot, RobotImages.PC_16x16},
                        {Keys.Auth.Process_Disabled, ComponentImages.Procedure_16x16},
                        {Keys.Auth.Robot_Disabled, RobotImages.PC_Error_16x16_Disabled},
                        {Keys.Auth.User_Disabled, AuthImages.User_Blue_16x16_Disabled},
                        {Keys.Auth.Users_Disabled, AuthImages.Users_16x16_Disabled},
                        {Keys.Auth.ServiceAccount, ComponentImages.Key_16x16},
                        {Keys.Auth.ServiceAccount_Disabled, ComponentImages.Key_16x16_Disabled}
                    });
                }
                return m_auth16;
            }
        }

        public static ImageList Auth_32x32
        {
            get
            {
                if (m_auth32 == null)
                {
                    m_auth32 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Auth.User, AuthImages.User_Blue_32x32},
                        {Keys.Auth.UserLocked, AuthImages.User_Blue_Lock_32x32},
                        {Keys.Auth.Users, AuthImages.Users_32x32},
                        {Keys.Auth.Process, ComponentImages.Procedure_32x32},
                        {Keys.Auth.Robot, RobotImages.PC_32x32},
                        {Keys.Auth.Process_Disabled, ComponentImages.Procedure_32x32},
                        {Keys.Auth.Robot_Disabled, RobotImages.PC_Error_32x32_Disabled},
                        {Keys.Auth.User_Disabled, AuthImages.User_Blue_32x32_Disabled},
                        {Keys.Auth.Users_Disabled, AuthImages.Users_32x32_Disabled},
                        {Keys.Auth.ServiceAccount, ComponentImages.Key_32x32},
                        {Keys.Auth.ServiceAccount_Disabled, ComponentImages.Key_32x32_Disabled}
                    });
                }
                return m_auth32;
            }
        }

        public static ImageList Web_16x16
        {
            get
            {
                if (m_web16 == null)
                {
                    m_web16 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Component.WebService, ComponentImages.Document_HTML_16x16}
                    });
                }
                return m_web16;
            }
        }

        public static ImageList Web_32x32
        {
            get
            {
                if (m_web32 == null)
                {
                    m_web32 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Component.WebService, ComponentImages.Document_HTML_32x32}
                    });
                }
                return m_web32;
            }
        }

        public static ImageList Publishing_16x16
        {
            get
            {
                if (m_publish16 == null)
                {
                    m_publish16 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Publishing.Published, ComponentImages.Structure_16x16}
                    });
                }
                return m_publish16;
            }
        }

        public static ImageList Filtering_16x16
        {
            get
            {
                if (m_filter16 == null)
                {
                    m_filter16 = Create(new clsOrderedDictionary<string, Image>()
                    {
                        {Keys.Filtering.Search, ToolImages.Filter_16x16},
                        {Keys.Filtering.Clear, ToolImages.Cross_16x16},
                        {Keys.Filtering.Clear_Disabled, ToolImages.Cross_16x16_Disabled},
                    });
                }
                return m_filter16;
            }
        }

        /// <summary>
        /// Overlays an Image onto a base image, returning the result.
        /// </summary>
        /// <param name="baseImage"></param>
        /// <param name="overlay"></param>
        /// <returns></returns>
        private static Bitmap Combine(Bitmap baseImage, Bitmap overlay)
        {


            Bitmap finalImage = new Bitmap(baseImage.Width, baseImage.Height, PixelFormat.Format32bppArgb);
            using (Graphics grD = Graphics.FromImage(finalImage))
            {
                grD.CompositingMode = CompositingMode.SourceOver;
                grD.DrawImage(baseImage, 0, 0);
                grD.DrawImage(overlay, 0, 0);
            }
            return finalImage;
           
        }

        #endregion
    }
}
