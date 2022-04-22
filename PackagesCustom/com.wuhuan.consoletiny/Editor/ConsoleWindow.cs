using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;

using UnityEditor.Networking.PlayerConnection;
using ConnectionGUILayout = UnityEditor.Networking.PlayerConnection.PlayerConnectionGUILayout;
using EditorGUI = UnityEditor.EditorGUI;
using EditorGUILayout = UnityEditor.EditorGUILayout;
using EditorGUIUtility = UnityEditor.EditorGUIUtility;
using CoreLog = UnityEditor;

namespace ConsoleTiny
{
    [EditorWindowTitle(title = "Console", useTypeNameAsIconName = true)]
    public class ConsoleWindow : EditorWindow, IHasCustomMenu
    {
        [MenuItem("Window/General/ConsoleT %#t", false, 7)]
        static void ShowConsole()
        {
            EditorWindow.GetWindow<ConsoleWindow>();
        }

        internal delegate void EntryDoubleClickedDelegate(LogEntry entry);
        private static bool m_UseMonospaceFont;
        private static Font m_MonospaceFont;
        private static int m_DefaultFontSize;

        //TODO: move this out of here
        internal class Constants
        {
            private static bool ms_Loaded;
            private static int ms_logStyleLineCount;
            public static GUIStyle Box;
            public static GUIStyle MiniButton;
            public static GUIStyle MiniButtonRight;
            public static GUIStyle LogStyle;
            public static GUIStyle WarningStyle;
            public static GUIStyle ErrorStyle;
            public static GUIStyle IconLogStyle;
            public static GUIStyle IconWarningStyle;
            public static GUIStyle IconErrorStyle;
            public static GUIStyle EvenBackground;
            public static GUIStyle OddBackground;
            public static GUIStyle MessageStyle;
            public static GUIStyle StatusError;
            public static GUIStyle StatusWarn;
            public static GUIStyle StatusLog;
            public static GUIStyle Toolbar;
            public static GUIStyle CountBadge;
            public static GUIStyle LogSmallStyle;
            public static GUIStyle WarningSmallStyle;
            public static GUIStyle ErrorSmallStyle;
            public static GUIStyle IconLogSmallStyle;
            public static GUIStyle IconWarningSmallStyle;
            public static GUIStyle IconErrorSmallStyle;

            public static readonly string ClearLabel = ("Clear");
            public static readonly string ClearOnPlayLabel = ("Clear on Play");
            public static readonly string ErrorPauseLabel = ("Error Pause");
            public static readonly string CollapseLabel = ("Collapse");
            public static readonly string StopForAssertLabel = ("Stop for Assert");
            public static readonly string StopForErrorLabel = ("Stop for Error");
            public static readonly string ClearOnBuildLabel = ("Clear on Build");
            public static readonly string FirstErrorLabel = ("First Error");
            public static readonly string CustomFiltersLabel = ("Custom Filters");
            public static readonly string ImportWatchingLabel = ("Import Watching");
            public static readonly GUIContent Clear = EditorGUIUtility.TrTextContent("Clear", "Clear console entries");
            public static readonly GUIContent ClearOnPlay = EditorGUIUtility.TrTextContent("Clear on Play");
            public static readonly GUIContent ClearOnBuild = EditorGUIUtility.TrTextContent("Clear on Build");
            public static readonly GUIContent ClearOnRecompile = EditorGUIUtility.TrTextContent("Clear on Recompile");
            public static readonly GUIContent Collapse = EditorGUIUtility.TrTextContent("Collapse", "Collapse identical entries");
            public static readonly GUIContent ErrorPause = EditorGUIUtility.TrTextContent("Error Pause", "Pause Play Mode on error");
            public static readonly GUIContent StopForAssert = EditorGUIUtility.TrTextContent("Stop for Assert");
            public static readonly GUIContent StopForError = EditorGUIUtility.TrTextContent("Stop for Error");
            public static readonly GUIContent UseMonospaceFont = EditorGUIUtility.TrTextContent("Use Monospace font");

            public static int LogStyleLineCount
            {
                get { return ms_logStyleLineCount; }
                set
                {
                    ms_logStyleLineCount = value;
                    LogEntries.wrapped.numberOfLines = value;

                    // If Constants hasn't been initialized yet we just skip this for now
                    // and let Init() call this for us in a bit.
                    if (!ms_Loaded)
                        return;
                    UpdateLogStyleFixedHeights();
                }
            }

            public static void Init()
            {
                if (ms_Loaded)
                    return;
                ms_Loaded = true;
                Box = "Box";


                MiniButton = "ToolbarButton";
                MiniButtonRight = "ToolbarButtonRight";
                Toolbar = "Toolbar";
                LogStyle = "CN EntryInfo";
                WarningStyle = "CN EntryWarn";
                ErrorStyle = "CN EntryError";

                EvenBackground = "CN EntryBackEven";
                OddBackground = "CN EntryBackodd";
                MessageStyle = "CN Message";
                StatusError = "CN StatusError";
                StatusWarn = "CN StatusWarn";
                StatusLog = "CN StatusInfo";
                CountBadge = "CN CountBadge";

                MessageStyle = new GUIStyle(MessageStyle);
                MessageStyle.onNormal.textColor = MessageStyle.active.textColor;
                MessageStyle.padding.top = 0;
                MessageStyle.padding.bottom = 0;
                var selectedStyle = new GUIStyle("MeTransitionSelect");
                MessageStyle.onNormal.background = selectedStyle.normal.background;

                bool isProSkin = EditorGUIUtility.isProSkin;
                IconLogStyle = "CN EntryInfo";
                IconWarningStyle = "CN EntryWarn";
                IconErrorStyle = "CN EntryError";
                LogSmallStyle = new GUIStyle("CN EntryInfo");
                WarningSmallStyle = new GUIStyle("CN EntryWarn");
                ErrorSmallStyle = new GUIStyle("CN EntryError");
                IconLogSmallStyle = new GUIStyle("CN EntryInfo");
                IconWarningSmallStyle = new GUIStyle("CN EntryWarn");
                IconErrorSmallStyle = new GUIStyle("CN EntryError");
                LogEntries.EntryWrapped.Constants.colorNamespace = isProSkin ? "6A87A7" : "66677E";
                LogEntries.EntryWrapped.Constants.colorClass = isProSkin ? "1A7ECD" : "0072A0";
                LogEntries.EntryWrapped.Constants.colorMethod = isProSkin ? "0D9DDC" : "335B89";
                LogEntries.EntryWrapped.Constants.colorParameters = isProSkin ? "4F7F9F" : "4C5B72";
                LogEntries.EntryWrapped.Constants.colorPath = isProSkin ? "375E68" : "7F8B90";
                LogEntries.EntryWrapped.Constants.colorFilename = isProSkin ? "4A6E8A" : "6285A1";
                LogEntries.EntryWrapped.Constants.colorNamespaceAlpha = isProSkin ? "4E5B6A" : "87878F";
                LogEntries.EntryWrapped.Constants.colorClassAlpha = isProSkin ? "2A577B" : "628B9B";
                LogEntries.EntryWrapped.Constants.colorMethodAlpha = isProSkin ? "246581" : "748393";
                LogEntries.EntryWrapped.Constants.colorParametersAlpha = isProSkin ? "425766" : "7D838B";
                LogEntries.EntryWrapped.Constants.colorPathAlpha = isProSkin ? "375860" : "8E989D";
                LogEntries.EntryWrapped.Constants.colorFilenameAlpha = isProSkin ? "4A6E8A" : "6285A1";


                LogSmallStyle.normal.background = null; LogSmallStyle.onNormal.background = null;
                WarningSmallStyle.normal.background = null; WarningSmallStyle.onNormal.background = null;
                ErrorSmallStyle.normal.background = null; ErrorSmallStyle.onNormal.background = null;
                IconLogSmallStyle.normal.background = null; IconLogSmallStyle.onNormal.background = null;
                IconWarningSmallStyle.normal.background = null; IconWarningSmallStyle.onNormal.background = null;
                IconErrorSmallStyle.normal.background = null; IconErrorSmallStyle.onNormal.background = null;


                // If the console window isn't open OnEnable() won't trigger so it will end up with 0 lines,
                // so we always make sure we read it up when we initialize here.
                LogStyleLineCount = EditorPrefs.GetInt("ConsoleWindowLogLineCount", 2);

                m_MonospaceFont = EditorGUIUtility.Load("Fonts/RobotoMono/RobotoMono-Regular.ttf") as Font;
                m_UseMonospaceFont = HasFlag(ConsoleFlags.UseMonospaceFont);
                m_DefaultFontSize = LogStyle.fontSize;
                SetFont();
            }

            internal static void UpdateLogStyleFixedHeights()
            {
                // Whenever we change the line height count or the styles are set we need to update the fixed height
                // of the following GuiStyles so the entries do not get cropped incorrectly.
                ErrorStyle.fixedHeight = (LogStyleLineCount * ErrorStyle.lineHeight) + ErrorStyle.border.top;
                WarningStyle.fixedHeight = (LogStyleLineCount * WarningStyle.lineHeight) + WarningStyle.border.top;
                LogStyle.fixedHeight = (LogStyleLineCount * LogStyle.lineHeight) + LogStyle.border.top;
            }
        }

        int m_LineHeight;
        int m_BorderHeight;

        bool m_HasUpdatedGuiStyles;
        Vector2 m_TextScroll = Vector2.zero;

        ListViewState m_ListView;
        ListViewState m_ListViewMessage;
        private int m_StacktraceLineContextClickRow;
        private int m_ActiveInstanceID = 0;
        bool m_DevBuild;
        private string[] m_SearchHistory = new[] { "" };
        private double m_NextRepaint = double.MaxValue;

        SplitterState spl = new SplitterState(new float[] { 70, 30 }, new int[] { 32, 32 }, null);

        static bool ms_LoadedIcons = false;
        static internal Texture2D iconInfo, iconWarn, iconError;
        static internal Texture2D iconInfoSmall, iconWarnSmall, iconErrorSmall, iconFirstErrorSmall;
        static internal Texture2D iconInfoMono, iconWarnMono, iconErrorMono, iconFirstErrorMono, iconCustomFiltersMono;
        static internal Texture2D[] iconCustomFiltersSmalls;

        int ms_LVHeight = 0;

        class ConsoleAttachToPlayerState : GeneralConnectionState
        {
            static class Content
            {
                public static GUIContent PlayerLogging = EditorGUIUtility.TrTextContent("Player Logging");
                public static GUIContent FullLog = EditorGUIUtility.TrTextContent("Full Log (Developer Mode Only)");
            }

            public ConsoleAttachToPlayerState(EditorWindow parentWindow, Action<string, EditorConnectionTarget?> connectedCallback = null) : base(parentWindow, connectedCallback)
            {
            }

            bool IsConnected()
            {
                return PlayerConnectionLogReceiver.instance.State != PlayerConnectionLogReceiver.ConnectionState.Disconnected;
            }

            void PlayerLoggingOptionSelected()
            {
                PlayerConnectionLogReceiver.instance.State = IsConnected() ? PlayerConnectionLogReceiver.ConnectionState.Disconnected : PlayerConnectionLogReceiver.ConnectionState.CleanLog;
            }

            bool IsLoggingFullLog()
            {
                return PlayerConnectionLogReceiver.instance.State == PlayerConnectionLogReceiver.ConnectionState.FullLog;
            }

            void FullLogOptionSelected()
            {
                PlayerConnectionLogReceiver.instance.State = IsLoggingFullLog() ? PlayerConnectionLogReceiver.ConnectionState.CleanLog : PlayerConnectionLogReceiver.ConnectionState.FullLog;
            }

            //public override void AddItemsToMenu(GenericMenu menu, Rect position)
            //{
            //    // option to turn logging and the connection on or of
            //    menu.AddItem(Content.PlayerLogging, IsConnected(), PlayerLoggingOptionSelected);
            //    if (IsConnected())
            //    {
            //        // All other options but the first are only available if logging is enabled
            //        menu.AddItem(Content.FullLog, IsLoggingFullLog(), FullLogOptionSelected);
            //        menu.AddSeparator("");
            //        base.AddItemsToMenu(menu, position);
            //    }
            //}
        }

        UnityEngine.Networking.PlayerConnection.IConnectionState m_ConsoleAttachToPlayerState;

        enum ConsoleFlags
        {
            Collapse = 1 << 0,
            ClearOnPlay = 1 << 1,
            ErrorPause = 1 << 2,
            Verbose = 1 << 3,
            StopForAssert = 1 << 4,
            StopForError = 1 << 5,
            Autoscroll = 1 << 6,
            LogLevelLog = 1 << 7,
            LogLevelWarning = 1 << 8,
            LogLevelError = 1 << 9,
            ShowTimestamp = 1 << 10,
            ClearOnBuild = 1 << 11,
            ClearOnRecompile = 1 << 12,
            UseMonospaceFont = 1 << 13,
        };

        static ConsoleWindow ms_ConsoleWindow = null;
        static internal void LoadIcons()
        {
            if (ms_LoadedIcons)
                return;

            ms_LoadedIcons = true;
            iconInfo = EditorGUIUtility.LoadIcon("console.infoicon");
            iconWarn = EditorGUIUtility.LoadIcon("console.warnicon");
            iconError = EditorGUIUtility.LoadIcon("console.erroricon");
            iconInfoSmall = EditorGUIUtility.LoadIcon("console.infoicon.sml");
            iconWarnSmall = EditorGUIUtility.LoadIcon("console.warnicon.sml");
            iconErrorSmall = EditorGUIUtility.LoadIcon("console.erroricon.sml");
            iconFirstErrorSmall = EditorGUIUtility.LoadIcon("sv_icon_dot14_sml");

            iconInfoMono = EditorGUIUtility.LoadIcon("console.infoicon.sml");
            iconWarnMono = EditorGUIUtility.LoadIcon("console.warnicon.inactive.sml");
            iconErrorMono = EditorGUIUtility.LoadIcon("console.erroricon.inactive.sml");
            iconFirstErrorMono = EditorGUIUtility.LoadIcon("sv_icon_dot8_sml");
            iconCustomFiltersMono = EditorGUIUtility.LoadIcon("sv_icon_dot0_sml");

            iconCustomFiltersSmalls = new Texture2D[7];
            for (int i = 0; i < 7; i++)
            {
                iconCustomFiltersSmalls[i] = EditorGUIUtility.LoadIcon("sv_icon_dot" + (i + 1) + "_sml");
            }
            Constants.Init();
        }

        public void DoLogChanged(string logString, string stackTrace, LogType type)
        {
            if (ms_ConsoleWindow == null)
                return;
            ms_ConsoleWindow.m_NextRepaint = EditorApplication.timeSinceStartup + 0.25f;
        }

        public ConsoleWindow()
        {
            position = new Rect(200, 200, 800, 400);
            m_ListView = new ListViewState(0, 0);
            m_ListViewMessage = new ListViewState(0, 14);
            m_StacktraceLineContextClickRow = -1;
        }

        void OnEnable()
        {

            if (m_ConsoleAttachToPlayerState == null)
                m_ConsoleAttachToPlayerState = new ConsoleAttachToPlayerState(this);

            MakeSureConsoleAlwaysOnlyOne();

            titleContent = EditorGUIUtility.TextContentWithIcon("Console", "UnityEditor.ConsoleWindow");
            titleContent = new GUIContent(titleContent) { text = "ConsoleT" };
            ms_ConsoleWindow = this;
            m_DevBuild = Unsupported.IsDeveloperMode();

            LogEntries.wrapped.searchHistory = m_SearchHistory;

            Constants.LogStyleLineCount = EditorPrefs.GetInt("ConsoleWindowLogLineCount", 2);
            Application.logMessageReceived += DoLogChanged;
        }

        void MakeSureConsoleAlwaysOnlyOne()
        {
            // make sure that console window is always open as only one.
            if (ms_ConsoleWindow != null)
            {
                // get the container window of this console window.
                ContainerWindow cw = ms_ConsoleWindow.m_Parent.window;

                // the container window must not be main view(prevent from quitting editor).
                if (cw.rootView.GetType() != typeof(MainView))
                    cw.Close();
            }
        }

        void OnDisable()
        {
            Application.logMessageReceived -= DoLogChanged;

            m_ConsoleAttachToPlayerState?.Dispose();
            m_ConsoleAttachToPlayerState = null;
            m_SearchHistory = LogEntries.wrapped.searchHistory;
            if (ms_ConsoleWindow == this)
                ms_ConsoleWindow = null;
        }

        void OnInspectorUpdate()
        {
            if (EditorApplication.timeSinceStartup > m_NextRepaint)
            {
                m_NextRepaint = double.MaxValue;
                Repaint();
            }
        }

        private int RowHeight
        {
            get
            {
                return (Constants.LogStyleLineCount * m_LineHeight) + m_BorderHeight;
            }
        }

        private static bool HasFlag(ConsoleFlags flags) { return (CoreLog.LogEntries.consoleFlags & (int)flags) != 0; }
        private static void SetFlag(ConsoleFlags flags, bool val) { CoreLog.LogEntries.SetConsoleFlag((int)flags, val); }

        private static Texture2D GetIconForErrorMode(ConsoleFlags flags, bool large)
        {
            // Errors
            if (flags == ConsoleFlags.LogLevelError)
                return large ? iconError : iconErrorSmall;
            // Warnings
            if (flags == ConsoleFlags.LogLevelWarning)
                return large ? iconWarn : iconWarnSmall;
            // Logs
            return large ? iconInfo : iconInfoSmall;
        }

        private static GUIStyle GetStyleForErrorMode(ConsoleFlags flags, bool isIcon, bool isSmall)
        {
            // Errors
            if (flags == ConsoleFlags.LogLevelError)
            {
                if (isIcon)
                {
                    if (isSmall)
                    {
                        return Constants.IconErrorSmallStyle;
                    }
                    return Constants.IconErrorStyle;
                }

                if (isSmall)
                {
                    return Constants.ErrorSmallStyle;
                }
                return Constants.ErrorStyle;
            }
            // Warnings
            if (flags == ConsoleFlags.LogLevelWarning)
            {
                if (isIcon)
                {
                    if (isSmall)
                    {
                        return Constants.IconWarningSmallStyle;
                    }
                    return Constants.IconWarningStyle;
                }

                if (isSmall)
                {
                    return Constants.WarningSmallStyle;
                }
                return Constants.WarningStyle;
            }
            // Logs
            if (isIcon)
            {
                if (isSmall)
                {
                    return Constants.IconLogSmallStyle;
                }
                return Constants.IconLogStyle;
            }

            if (isSmall)
            {
                return Constants.LogSmallStyle;
            }
            return Constants.LogStyle;
        }

        void SetActiveEntry(int selectedIndex)
        {
            m_ListViewMessage.row = -1;
            m_ListViewMessage.scrollPos.y = 0;
            if (selectedIndex != -1)
            {
                var instanceID = LogEntries.wrapped.SetSelectedEntry(selectedIndex);
                // ping object referred by the log entry
                if (m_ActiveInstanceID != instanceID)
                {
                    m_ActiveInstanceID = instanceID;
                    if (instanceID != 0)
                        EditorGUIUtility.PingObject(instanceID);
                }
            }
            else
            {
                m_ActiveInstanceID = 0;
                m_ListView.row = -1;
            }
        }

        void UpdateListView()
        {
            m_HasUpdatedGuiStyles = true;
            int newRowHeight = RowHeight;

            // We reset the scroll list to auto scrolling whenever the log entry count is modified
            m_ListView.rowHeight = newRowHeight;
            m_ListView.row = -1;
            m_ListView.scrollPos.y = LogEntries.wrapped.GetCount() * newRowHeight;
        }

        void OnGUI()
        {
            Event e = Event.current;
            LoadIcons();
            LogEntries.wrapped.UpdateEntries();

            if (!m_HasUpdatedGuiStyles)
            {
                m_LineHeight = Mathf.RoundToInt(Constants.ErrorStyle.lineHeight);
                m_BorderHeight = Constants.ErrorStyle.border.top + Constants.ErrorStyle.border.bottom;
                UpdateListView();
            }

            GUILayout.BeginHorizontal(Constants.Toolbar);

            if (LogEntries.wrapped.importWatching)
            {
                LogEntries.wrapped.importWatching = GUILayout.Toggle(LogEntries.wrapped.importWatching, Constants.ImportWatchingLabel, Constants.MiniButton);
            }

            // Clear button and clearing options
            bool clearClicked = false;
            if (EditorGUILayout.DropDownToggle(ref clearClicked, Constants.Clear, EditorStyles.toolbarDropDownToggle))
            {
                var clearOnPlay = HasFlag(ConsoleFlags.ClearOnPlay);
                var clearOnBuild = HasFlag(ConsoleFlags.ClearOnBuild);
                var clearOnRecompile = HasFlag(ConsoleFlags.ClearOnRecompile);

                GenericMenu menu = new GenericMenu();
                menu.AddItem(Constants.ClearOnPlay, clearOnPlay, () => { SetFlag(ConsoleFlags.ClearOnPlay, !clearOnPlay); });
                menu.AddItem(Constants.ClearOnBuild, clearOnBuild, () => { SetFlag(ConsoleFlags.ClearOnBuild, !clearOnBuild); });
                menu.AddItem(Constants.ClearOnRecompile, clearOnRecompile, () => { SetFlag(ConsoleFlags.ClearOnRecompile, !clearOnRecompile); });
                var rect = GUILayoutUtility.GetLastRect();
                rect.y += EditorGUIUtility.singleLineHeight;
                menu.DropDown(rect);
            }
            if (clearClicked)
            {
                LogEntries.Clear();
                GUIUtility.keyboardControl = 0;
            }

            int currCount = LogEntries.wrapped.GetCount();
            if (m_ListView.totalRows != currCount && m_ListView.totalRows > 0)
            {
                // scroll bar was at the bottom?
                if (m_ListView.scrollPos.y >= m_ListView.rowHeight * m_ListView.totalRows - ms_LVHeight)
                {
                    m_ListView.scrollPos.y = currCount * RowHeight - ms_LVHeight;
                }
            }

            if (LogEntries.wrapped.searchFrame)
            {
                LogEntries.wrapped.searchFrame = false;
                int selectedIndex = LogEntries.wrapped.GetSelectedEntryIndex();
                if (selectedIndex != -1)
                {
                    int showIndex = selectedIndex + 1;
                    if (currCount > showIndex)
                    {
                        int showCount = ms_LVHeight / RowHeight;
                        showIndex = showIndex + showCount / 2;
                    }
                    m_ListView.scrollPos.y = showIndex * RowHeight - ms_LVHeight;
                }
            }

            EditorGUILayout.Space();

            bool wasCollapsed = LogEntries.wrapped.collapse;
            LogEntries.wrapped.collapse = GUILayout.Toggle(wasCollapsed, Constants.CollapseLabel, Constants.MiniButton);

            bool collapsedChanged = (wasCollapsed != LogEntries.wrapped.collapse);
            if (collapsedChanged)
            {
                // unselect if collapsed flag changed
                m_ListView.row = -1;

                // scroll to bottom
                m_ListView.scrollPos.y = LogEntries.wrapped.GetCount() * RowHeight;
            }

            SetFlag(ConsoleFlags.ErrorPause, GUILayout.Toggle(HasFlag(ConsoleFlags.ErrorPause), Constants.ErrorPauseLabel, Constants.MiniButton));
            ConnectionGUILayout.ConnectionTargetSelectionDropdown(m_ConsoleAttachToPlayerState, EditorStyles.toolbarDropDown);

            EditorGUILayout.Space();

            if (m_DevBuild)
            {
                GUILayout.FlexibleSpace();
                SetFlag(ConsoleFlags.StopForAssert, GUILayout.Toggle(HasFlag(ConsoleFlags.StopForAssert), Constants.StopForAssertLabel, Constants.MiniButton));
                SetFlag(ConsoleFlags.StopForError, GUILayout.Toggle(HasFlag(ConsoleFlags.StopForError), Constants.StopForErrorLabel, Constants.MiniButton));
            }

            GUILayout.FlexibleSpace();

            // Search bar
            GUILayout.Space(4f);
            SearchField(e);

            int errorCount = 0, warningCount = 0, logCount = 0;
            LogEntries.wrapped.GetCountsByType(ref errorCount, ref warningCount, ref logCount);
            EditorGUI.BeginChangeCheck();
            bool setLogFlag = GUILayout.Toggle(LogEntries.wrapped.HasFlag((int)ConsoleFlags.LogLevelLog), new GUIContent((logCount <= 999 ? logCount.ToString() : "999+"), logCount > 0 ? iconInfoSmall : iconInfoMono), Constants.MiniButton);
            bool setWarningFlag = GUILayout.Toggle(LogEntries.wrapped.HasFlag((int)ConsoleFlags.LogLevelWarning), new GUIContent((warningCount <= 999 ? warningCount.ToString() : "999+"), warningCount > 0 ? iconWarnSmall : iconWarnMono), Constants.MiniButton);
            bool setErrorFlag = GUILayout.Toggle(LogEntries.wrapped.HasFlag((int)ConsoleFlags.LogLevelError), new GUIContent((errorCount <= 999 ? errorCount.ToString() : "999+"), errorCount > 0 ? iconErrorSmall : iconErrorMono), Constants.MiniButtonRight);
            // Active entry index may no longer be valid
            if (EditorGUI.EndChangeCheck())
            { }

            LogEntries.wrapped.SetFlag((int)ConsoleFlags.LogLevelLog, setLogFlag);
            LogEntries.wrapped.SetFlag((int)ConsoleFlags.LogLevelWarning, setWarningFlag);
            LogEntries.wrapped.SetFlag((int)ConsoleFlags.LogLevelError, setErrorFlag);

            if (GUILayout.Button(new GUIContent(errorCount > 0 ? iconFirstErrorSmall : iconFirstErrorMono, Constants.FirstErrorLabel), Constants.MiniButton))
            {
                int firstErrorIndex = LogEntries.wrapped.GetFirstErrorEntryIndex();
                if (firstErrorIndex != -1)
                {
                    SetActiveEntry(firstErrorIndex);
                    LogEntries.wrapped.searchFrame = true;
                }
            }

            GUILayout.EndHorizontal();

            //Console Entries
            SplitterGUILayout.BeginVerticalSplit(spl);
            int rowHeight = RowHeight;
            EditorGUIUtility.SetIconSize(new Vector2(rowHeight, rowHeight));
            GUIContent tempContent = new GUIContent();
            int id = GUIUtility.GetControlID(0);
            int rowDoubleClicked = -1;

            /////@TODO: Make Frame selected work with ListViewState
            using (new GettingLogEntriesScope(m_ListView))
            {
                int selectedRow = -1;
                bool openSelectedItem = false;
                bool collapsed = LogEntries.wrapped.collapse;

                foreach (ListViewElement el in ListViewGUI.ListView(m_ListView, ListViewOptions.wantsRowMultiSelection, Constants.Box))
                {
                    if (e.type == EventType.MouseDown && e.button == 0 && el.position.Contains(e.mousePosition))
                    {
                        m_ListView.row = el.row;
                        selectedRow = el.row;
                        if (e.clickCount == 2)
                            openSelectedItem = true;
                    }
                    else if (e.type == EventType.Repaint)
                    {
                        int mode = 0;
                        int entryCount = 0;
                        int searchIndex = 0;
                        int searchEndIndex = 0;
                        string text = LogEntries.wrapped.GetEntryLinesAndFlagAndCount(el.row, ref mode, ref entryCount,
                            ref searchIndex, ref searchEndIndex);
                        ConsoleFlags flag = (ConsoleFlags)mode;
                        bool isSelected = LogEntries.wrapped.IsEntrySelected(el.row);

                        // offset value in x for icon and text
                        var offset = Constants.LogStyleLineCount == 1 ? 4 : 8;
                        
                        // Draw the background
                        GUIStyle s = el.row % 2 == 0 ? Constants.OddBackground : Constants.EvenBackground;
                        s.Draw(el.position, false, false, isSelected, false);


                        // Draw the icon
                        GUIStyle iconStyle = GetStyleForErrorMode(flag, true, Constants.LogStyleLineCount == 1);
                        Rect iconRect = el.position;
                        iconRect.x += offset;
                        iconRect.y += 2;

                        iconStyle.Draw(iconRect, false, false, isSelected, false);

                        // Draw the text
                        tempContent.text = text;
                        GUIStyle errorModeStyle = GetStyleForErrorMode(flag, false, Constants.LogStyleLineCount == 1);

                        var textRect = el.position;
                        textRect.x += offset;

                        if (string.IsNullOrEmpty(LogEntries.wrapped.searchString) || searchIndex == -1 || searchIndex >= text.Length)
                        {
                            errorModeStyle.Draw(textRect, tempContent, id, m_ListView.row == el.row);
                        }
                        else if (text != null)
                        {
                            //the whole text contains the searchtext, we have to know where it is
                            int startIndex = text.IndexOf(LogEntries.wrapped.searchString, StringComparison.OrdinalIgnoreCase);
                            if (startIndex == -1) // the searchtext is not in the visible text, we don't show the selection
                                errorModeStyle.Draw(el.position, tempContent, id, isSelected);
                            else
                            errorModeStyle.DrawWithTextSelection(el.position, tempContent, GUIUtility.keyboardControl, searchIndex, searchEndIndex);
                        }

                        if (collapsed)
                        {
                            Rect badgeRect = el.position;
                            tempContent.text = entryCount.ToString(CultureInfo.InvariantCulture);
                            Vector2 badgeSize = Constants.CountBadge.CalcSize(tempContent);
                            if (Constants.CountBadge.fixedHeight > 0)
                                badgeSize.y = Constants.CountBadge.fixedHeight;
                            badgeRect.xMin = badgeRect.xMax - badgeSize.x;
                            badgeRect.yMin += ((badgeRect.yMax - badgeRect.yMin) - badgeSize.y) * 0.5f;
                            badgeRect.x -= 5f;
                            GUI.Label(badgeRect, tempContent, Constants.CountBadge);
                        }
                    }
                }

                if (selectedRow != -1)
                {
                    if (m_ListView.scrollPos.y >= m_ListView.rowHeight * m_ListView.totalRows - ms_LVHeight)
                        m_ListView.scrollPos.y = m_ListView.rowHeight * m_ListView.totalRows - ms_LVHeight - 1;
                }

                // Make sure the selected entry is up to date
                if (m_ListView.totalRows == 0 || m_ListView.row >= m_ListView.totalRows || m_ListView.row < 0)
                {
                    if (LogEntries.wrapped.GetSelectedEntryText().Length != 0)
                    {
                        SetActiveEntry(-1);
                    }
                }
                else
                {
                    if (m_ListView.selectionChanged)
                    {
                        SetActiveEntry(m_ListView.row);
                    }

                }

                // Open entry using return key
                if ((GUIUtility.keyboardControl == m_ListView.ID) && (e.type == EventType.KeyDown) 
                    && (e.keyCode == KeyCode.Return) && (m_ListView.row != 0))
                {
                    selectedRow = m_ListView.row;
                    openSelectedItem = true;
                }

                if (e.type != EventType.Layout && ListViewGUI.ilvState.rectHeight != 1)
                    ms_LVHeight = ListViewGUI.ilvState.rectHeight;

                if (openSelectedItem)
                {
                    rowDoubleClicked = selectedRow;
                    e.Use();
                }

                if (selectedRow != -1)
                {
                    SetActiveEntry(selectedRow);
                }
            }

            // Prevent dead locking in EditorMonoConsole by delaying callbacks (which can log to the console) until after LogEntries.EndGettingEntries() has been
            // called (this releases the mutex in EditorMonoConsole so logging again is allowed). Fix for case 1081060.
            if (rowDoubleClicked != -1)
                LogEntries.wrapped.StacktraceListView_RowGotDoubleClicked();
            //-- Uncomment the following line and comment the above line to let Unity handle file opening
                //CoreLog.LogEntries.RowGotDoubleClicked(rowDoubleClicked); 

            EditorGUIUtility.SetIconSize(Vector2.zero);

            LogEntries.wrapped.UpdateStacktraceListView();

            // Display active text (We want word wrapped text with a vertical scrollbar)

            m_TextScroll = GUILayout.BeginScrollView(m_TextScroll, Constants.Box);

            string stackWithHyperlinks = StacktraceWithHyperlinks(LogEntries.wrapped.GetSelectedEntryText(), 0);
            var guic = new GUIContent(stackWithHyperlinks);
            float height = Constants.MessageStyle.CalcHeight(guic, position.width);
            EditorGUILayout.SelectableLabel(stackWithHyperlinks, Constants.MessageStyle,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true), GUILayout.MinHeight(height + 10));

            GUILayout.EndScrollView();
            SplitterGUILayout.EndVerticalSplit();

            // Copy & Paste selected item
            if ((e.type == EventType.ValidateCommand || e.type == EventType.ExecuteCommand) && e.commandName == "Copy")
            {
                if (e.type == EventType.ExecuteCommand)
                    LogEntries.wrapped.StacktraceListView_CopyAll();
                e.Use();
            }
        }


        internal static string StacktraceWithHyperlinks(string stacktraceText, int callstackTextStart)
        {
            System.Text.StringBuilder textWithHyperlinks = new System.Text.StringBuilder();
            textWithHyperlinks.Append(stacktraceText.Substring(0, callstackTextStart));
            var lines = stacktraceText.Substring(callstackTextStart).Split(new string[] { "\n" }, StringSplitOptions.None);
            for (int i = 0; i < lines.Length; ++i)
            {
                string textBeforeFilePath = ") (at ";
                int filePathIndex = lines[i].IndexOf(textBeforeFilePath, StringComparison.Ordinal);
                if (filePathIndex > 0)
                {
                    filePathIndex += textBeforeFilePath.Length;
                    if (lines[i][filePathIndex] != '<') // sometimes no url is given, just an id between <>, we can't do an hyperlink
                    {
                        string filePathPart = lines[i].Substring(filePathIndex);
                        int lineIndex = filePathPart.LastIndexOf(":", StringComparison.Ordinal); // LastIndex because the url can contain ':' ex:"C:"
                        if (lineIndex > 0)
                        {
                            int endLineIndex = filePathPart.LastIndexOf(")", StringComparison.Ordinal); // LastIndex because files or folder in the url can contain ')'
                            if (endLineIndex > 0)
                            {
                                string lineString =
                                    filePathPart.Substring(lineIndex + 1, (endLineIndex) - (lineIndex + 1));
                                string filePath = filePathPart.Substring(0, lineIndex);

                                textWithHyperlinks.Append(lines[i].Substring(0, filePathIndex));
                                textWithHyperlinks.Append("<a href=\"" + filePath + "\"" + " line=\"" + lineString + "\">");
                                textWithHyperlinks.Append(filePath + ":" + lineString);
                                textWithHyperlinks.Append("</a>)\n");

                                continue; // continue to evade the default case
                            }
                        }
                    }
                }
                // default case if no hyperlink : we just write the line
                textWithHyperlinks.Append(lines[i] + "\n");
            }
            // Remove the last \n
            if (textWithHyperlinks.Length > 0) // textWithHyperlinks always ends with \n if it is not empty
                textWithHyperlinks.Remove(textWithHyperlinks.Length - 1, 1);

            return textWithHyperlinks.ToString();
        }

        private void SearchField(Event e)
        {
            string searchBarName = "SearchFilter";
            if (e.commandName == "Find")
            {
                if (e.type == EventType.ExecuteCommand)
                {
                    EditorGUI.FocusTextInControl(searchBarName);
                }

                if (e.type != EventType.Layout)
                    e.Use();
            }

            string searchText = LogEntries.wrapped.searchString;
            if (e.type == EventType.KeyDown)
            {
                if (e.keyCode == KeyCode.Escape)
                {
                    searchText = string.Empty;
                    GUIUtility.keyboardControl = m_ListView.ID;
                    Repaint();
                }
                else if ((e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow) &&
                         GUI.GetNameOfFocusedControl() == searchBarName)
                {
                    GUIUtility.keyboardControl = m_ListView.ID;
                }
            }

            GUI.SetNextControlName(searchBarName);
            Rect rect = GUILayoutUtility.GetRect(0, EditorGUILayout.kLabelFloatMaxW * 1.5f, EditorGUI.kSingleLineHeight,
                EditorGUI.kSingleLineHeight, EditorStyles.toolbarSearchField, GUILayout.MinWidth(100),
                GUILayout.MaxWidth(300));

            bool showHistory = LogEntries.wrapped.searchHistory[0].Length != 0;
            Rect popupPosition = rect;
            popupPosition.width = 20;
            if (showHistory && Event.current.type == EventType.MouseDown && popupPosition.Contains(Event.current.mousePosition))
            {
                GUIUtility.keyboardControl = 0;
                EditorUtility.DisplayCustomMenu(rect, EditorGUIUtility.TrTempContent(LogEntries.wrapped.searchHistory), -1, OnSetFilteringHistoryCallback, null);
                Event.current.Use();
            }

            LogEntries.wrapped.searchString = EditorGUI.ToolbarSearchField(
                rect, searchText, showHistory);

            if (GUILayout.Button(new GUIContent(iconCustomFiltersMono, Constants.CustomFiltersLabel), EditorStyles.toolbarDropDown))
            {
                Rect buttonRect = rect;
                buttonRect.x += buttonRect.width;
                var menuData = new CustomFiltersItemProvider(LogEntries.wrapped.customFilters);
                var flexibleMenu = new FlexibleMenu(menuData, -1, new CustomFiltersModifyItemUI(), null);
                PopupWindow.Show(buttonRect, flexibleMenu);
            }

            int iconIndex = 0;
            foreach (var filter in LogEntries.wrapped.customFilters.filters)
            {
                if (iconIndex >= 7)
                {
                    iconIndex = 0;
                }
                filter.toggle = GUILayout.Toggle(filter.toggle, new GUIContent(filter.filter, iconCustomFiltersSmalls[iconIndex++]), Constants.MiniButton);
            }
        }

        private void OnSetFilteringHistoryCallback(object userData, string[] options, int selected)
        {
            LogEntries.wrapped.searchString = options[selected];
        }

        public struct StackTraceLogTypeData
        {
            public LogType logType;
            public StackTraceLogType stackTraceLogType;
        }

        public void ToggleLogStackTraces(object userData)
        {
            StackTraceLogTypeData data = (StackTraceLogTypeData)userData;
            PlayerSettings.SetStackTraceLogType(data.logType, data.stackTraceLogType);
        }

        public void ToggleLogStackTracesForAll(object userData)
        {
            foreach (LogType logType in Enum.GetValues(typeof(LogType)))
                PlayerSettings.SetStackTraceLogType(logType, (StackTraceLogType)userData);
        }

        public void AddItemsToMenu(GenericMenu menu)
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
                menu.AddItem(EditorGUIUtility.TrTextContent("Open Player Log"), false, UnityEditorInternal.InternalEditorUtility.OpenPlayerConsole);
            menu.AddItem(EditorGUIUtility.TrTextContent("Open Editor Log"), false, UnityEditorInternal.InternalEditorUtility.OpenEditorConsole);
            menu.AddItem(EditorGUIUtility.TrTextContent("Export Console Log"), false, LogEntries.wrapped.ExportLog);
            menu.AddItem(EditorGUIUtility.TrTextContent("Import Console Log"), false, LogEntries.wrapped.ImportLog);

            menu.AddItem(EditorGUIUtility.TrTextContent("Show Timestamp"), LogEntries.wrapped.showTimestamp, SetTimestamp);

            for (int i = 1; i <= 10; ++i)
            {
                var lineString = i == 1 ? "Line" : "Lines";
                menu.AddItem(new GUIContent(string.Format("Log Entry/{0} {1}", i, lineString)), i == Constants.LogStyleLineCount, SetLogLineCount, i);
            }

            menu.AddItem(Constants.UseMonospaceFont, m_UseMonospaceFont, OnFontButtonValueChange);

            AddStackTraceLoggingMenu(menu);
        }

        private static void OnFontButtonValueChange()
        {
            m_UseMonospaceFont = !m_UseMonospaceFont;
            SetFlag(ConsoleFlags.UseMonospaceFont, m_UseMonospaceFont);
            SetFont();
        }

        private static void SetFont()
        {
            var styles = new[]
            {
                Constants.LogStyle,
                Constants.LogSmallStyle,
                Constants.WarningStyle,
                Constants.WarningSmallStyle,
                Constants.ErrorStyle,
                Constants.ErrorSmallStyle,
                Constants.MessageStyle,
            };

            Font font = m_UseMonospaceFont ? m_MonospaceFont : null;

            foreach (var style in styles)
            {
                style.font = font;
                style.fontSize = m_DefaultFontSize;
            }

            // Make sure to update the fixed height so the entries do not get cropped incorrectly.
            Constants.UpdateLogStyleFixedHeights();
        }

        private void SetTimestamp()
        {
            LogEntries.wrapped.showTimestamp = !LogEntries.wrapped.showTimestamp;
        }

        private void SetLogLineCount(object obj)
        {
            int count = (int)obj;
            EditorPrefs.SetInt("ConsoleWindowLogLineCount", count);
            Constants.LogStyleLineCount = count;

            UpdateListView();
        }

        private void AddStackTraceLoggingMenu(GenericMenu menu)
        {
            // TODO: Maybe remove this, because it basically duplicates UI in PlayerSettings
            foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            {
                foreach (StackTraceLogType stackTraceLogType in Enum.GetValues(typeof(StackTraceLogType)))
                {
                    StackTraceLogTypeData data;
                    data.logType = logType;
                    data.stackTraceLogType = stackTraceLogType;

                    menu.AddItem(EditorGUIUtility.TrTextContent("Stack Trace Logging/" + logType + "/" + stackTraceLogType), PlayerSettings.GetStackTraceLogType(logType) == stackTraceLogType,
                        ToggleLogStackTraces, data);
                }
            }

            int stackTraceLogTypeForAll = (int)PlayerSettings.GetStackTraceLogType(LogType.Log);
            foreach (LogType logType in Enum.GetValues(typeof(LogType)))
            {
                if (PlayerSettings.GetStackTraceLogType(logType) != (StackTraceLogType)stackTraceLogTypeForAll)
                {
                    stackTraceLogTypeForAll = -1;
                    break;
                }
            }

            foreach (StackTraceLogType stackTraceLogType in Enum.GetValues(typeof(StackTraceLogType)))
            {
                menu.AddItem(EditorGUIUtility.TrTextContent("Stack Trace Logging/All/" + stackTraceLogType), (StackTraceLogType)stackTraceLogTypeForAll == stackTraceLogType,
                    ToggleLogStackTracesForAll, stackTraceLogType);
            }
        }
    }

    internal class GettingLogEntriesScope : IDisposable
    {
        private bool m_Disposed;

        public GettingLogEntriesScope(ListViewState listView)
        {
            listView.totalRows = LogEntries.wrapped.GetCount();
        }

        public void Dispose()
        {
            if (m_Disposed)
                return;
            m_Disposed = true;
        }

        ~GettingLogEntriesScope()
        {
            if (!m_Disposed)
                Debug.LogError("Scope was not disposed! You should use the 'using' keyword or manually call Dispose.");
        }
    }

    #region CustomFilters

    internal class CustomFiltersItemProvider : IFlexibleMenuItemProvider
    {
        private readonly LogEntries.EntryWrapped.CustomFiltersGroup m_Groups;

        public CustomFiltersItemProvider(LogEntries.EntryWrapped.CustomFiltersGroup groups)
        {
            m_Groups = groups;
        }

        public int Count()
        {
            return m_Groups.filters.Count;
        }

        public object GetItem(int index)
        {
            return m_Groups.filters[index].filter;
        }

        public int Add(object obj)
        {
            m_Groups.filters.Add(new LogEntries.EntryWrapped.CustomFiltersItem() { filter = (string)obj, changed = false });
            m_Groups.Save();
            return Count() - 1;
        }

        public void Replace(int index, object newPresetObject)
        {
            m_Groups.filters[index].filter = (string)newPresetObject;
            m_Groups.Save();
        }

        public void Remove(int index)
        {
            if (m_Groups.filters[index].toggle)
            {
                m_Groups.changed = true;
            }
            m_Groups.filters.RemoveAt(index);
            m_Groups.Save();
        }

        public object Create()
        {
            return "log";
        }

        public void Move(int index, int destIndex, bool insertAfterDestIndex)
        {
            Debug.LogError("Missing impl");
        }

        public string GetName(int index)
        {
            return m_Groups.filters[index].filter;
        }

        public bool IsModificationAllowed(int index)
        {
            return true;
        }

        public int[] GetSeperatorIndices()
        {
            return new int[0];
        }
    }

    internal class CustomFiltersModifyItemUI : FlexibleMenuModifyItemUI
    {
        private static class Styles
        {
            public static GUIContent headerAdd = EditorGUIUtility.TrTextContent("Add");
            public static GUIContent headerEdit = EditorGUIUtility.TrTextContent("Edit");
            public static GUIContent optionalText = EditorGUIUtility.TrTextContent("Search");
            public static GUIContent ok = EditorGUIUtility.TrTextContent("OK");
            public static GUIContent cancel = EditorGUIUtility.TrTextContent("Cancel");
        }

        private string m_TextSearch;

        public override void OnClose()
        {
            m_TextSearch = null;
            base.OnClose();
        }

        public override Vector2 GetWindowSize()
        {
            return new Vector2(330f, 80f);
        }

        public override void OnGUI(Rect rect)
        {
            string itemValue = m_Object as string;
            if (itemValue == null)
            {
                Debug.LogError("Invalid object");
                return;
            }

            if (m_TextSearch == null)
            {
                m_TextSearch = itemValue;
            }

            const float kColumnWidth = 70f;
            const float kSpacing = 10f;

            GUILayout.Space(3);
            GUILayout.Label(m_MenuType == MenuType.Add ? Styles.headerAdd : Styles.headerEdit, EditorStyles.boldLabel);

            Rect seperatorRect = GUILayoutUtility.GetRect(1, 1);
            FlexibleMenu.DrawRect(seperatorRect,
                (EditorGUIUtility.isProSkin)
                    ? new Color(0.32f, 0.32f, 0.32f, 1.333f)
                    : new Color(0.6f, 0.6f, 0.6f, 1.333f));                      // dark : light
            GUILayout.Space(4);

            // Optional text
            GUILayout.BeginHorizontal();
            GUILayout.Label(Styles.optionalText, GUILayout.Width(kColumnWidth));
            GUILayout.Space(kSpacing);
            m_TextSearch = EditorGUILayout.TextField(m_TextSearch);
            GUILayout.EndHorizontal();

            GUILayout.Space(5f);

            // Cancel, Ok
            GUILayout.BeginHorizontal();
            GUILayout.Space(10);
            if (GUILayout.Button(Styles.cancel))
            {
                editorWindow.Close();
            }

            if (GUILayout.Button(Styles.ok))
            {
                var textSearch = m_TextSearch.Trim();
                if (!string.IsNullOrEmpty(textSearch))
                {
                    m_Object = m_TextSearch;
                    Accepted();
                    editorWindow.Close();
                }
            }
            GUILayout.Space(10);
            GUILayout.EndHorizontal();
        }
    }

    #endregion
}
