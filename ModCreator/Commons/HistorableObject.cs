using ModCreator.Attributes;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Reflection;
using System;

namespace ModCreator.Commons
{
    [NotifyMethod(nameof(WriteHistory))]
    public abstract class HistorableObject : AutoNotifiableObject
    {
        public const int MAX_HIST_TIMES = 8;

        public static readonly JsonSerializerSettings JsonSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto,
            Formatting = Formatting.None,
            NullValueHandling = NullValueHandling.Ignore,
            ObjectCreationHandling = ObjectCreationHandling.Replace
        };

        [JsonIgnore, IgnoredProperty]
        public List<string> Histories { get; } = [];

        [JsonIgnore, IgnoredProperty]
        public bool CanUndo => _histIndex > 0;

        [JsonIgnore, IgnoredProperty]
        public bool CanRedo => _histIndex < MAX_HIST_TIMES - 1;

        private bool _stopHistoryRecording = false;
        private int _histIndex = MAX_HIST_TIMES - 1;

        [Obsolete]
        public void WriteHistory(object obj, PropertyInfo prop, object oldValue, object newValue) {
            if (IsUpdated())
            {
                if (_stopHistoryRecording)
                {
                    _stopHistoryRecording = false;
                    return;
                }
                if (_histIndex < MAX_HIST_TIMES - 1)
                {
                    Histories.RemoveRange(_histIndex + 1, Histories.Count - (_histIndex + 1));
                }

                var value = JsonConvert.SerializeObject(this, JsonSettings);
                Histories.AddRange(Enumerable.Repeat(value, Math.Max(1, MAX_HIST_TIMES - Histories.Count)));
                if (Histories.Count > MAX_HIST_TIMES)
                {
                    Histories.RemoveAt(0);
                }

                _histIndex = MAX_HIST_TIMES - 1;
            }
        }

        public bool IsUpdated()
        {
            return Histories.Count == 0 || JsonConvert.SerializeObject(this, JsonSettings) != Histories[_histIndex];
        }

        public void Undo()
        {
            if (CanUndo)
            {
                _stopHistoryRecording = true;
                var state = Histories[--_histIndex];
                JsonConvert.PopulateObject(state, this, JsonSettings);
            }
        }

        public void Redo()
        {
            if (CanRedo)
            {
                _stopHistoryRecording = true;
                var state = Histories[++_histIndex];
                JsonConvert.PopulateObject(state, this, JsonSettings);
            }
        }
    }
}