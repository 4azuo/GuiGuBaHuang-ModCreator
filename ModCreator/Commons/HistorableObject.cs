using ModCreator.Attributes;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Threading;

namespace ModCreator.Commons
{
    //[SetterAspect]
    public abstract class HistorableObject : AutoNotifiableObject, IDisposable
    {
        public const int MAX_HIST_TIMES = 8;
        public const int AUTO_BACKUP_PERIOD = 3000;

        public static new readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
            ReferenceLoopHandling = ReferenceLoopHandling.Serialize
        };

        [JsonIgnore, IgnoredProperty]
        public DispatcherTimer AutoUpdateTimer { get; } = new(TimeSpan.FromMilliseconds(AUTO_BACKUP_PERIOD), DispatcherPriority.Background, (s, e) => { }, Application.Current.Dispatcher);

        [JsonIgnore, IgnoredProperty]
        public List<string> UndoHistories { get; } = [];

        [JsonIgnore, IgnoredProperty]
        public List<string> RedoHistories { get; } = [];

        [JsonIgnore]
        public bool CanUndo => UndoHistories.Count > 1;

        [JsonIgnore]
        public bool CanRedo => RedoHistories.Count > 0;

        public HistorableObject()
        {
            // Setup auto update timer
            AutoUpdateTimer.Tick += AutoUpdateTimer_Tick;
            AutoUpdateTimer.Start();
        }

        public new void Dispose()
        {
            base.Dispose();
            AutoUpdateTimer.Stop();
            AutoUpdateTimer.IsEnabled = false;
            AutoUpdateTimer.Tick -= AutoUpdateTimer_Tick;
        }

        private void AutoUpdateTimer_Tick(object sender, EventArgs e)
        {
            var curState = JsonConvert.SerializeObject(this, JsonSettings);
            var lastState = UndoHistories.Count > 0 ? UndoHistories.Last() : null;
            if (curState != lastState)
            {
                UndoHistories.Add(curState);
                if (UndoHistories.Count > MAX_HIST_TIMES)
                    UndoHistories.RemoveAt(0);
                Notify(nameof(CanUndo));
                Notify(nameof(CanRedo));
            }
        }

        public void Undo()
        {
            if (CanUndo)
            {
                var lastState = UndoHistories[UndoHistories.Count - 1];
                var oldState = UndoHistories[UndoHistories.Count - 2];
                UndoHistories.RemoveRange(UndoHistories.Count - 1, 1);
                RedoHistories.Add(lastState);
                JsonConvert.PopulateObject(oldState, this, JsonSettings);
                Notify(nameof(CanUndo));
                Notify(nameof(CanRedo));
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                var state = RedoHistories.Last();
                RedoHistories.RemoveRange(RedoHistories.Count - 1, 1);
                UndoHistories.Add(state);
                JsonConvert.PopulateObject(state, this, JsonSettings);
                Notify(nameof(CanUndo));
                Notify(nameof(CanRedo));
            }
        }
    }
}