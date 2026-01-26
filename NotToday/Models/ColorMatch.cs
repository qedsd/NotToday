using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace NotToday.Models
{
    public class ColorMatch : ObservableObject
    {
        public ColorMatch() { }
        public ColorMatch(LocalIntelColor color)
        {
            Color = color;
        }
        public LocalIntelColor Color {  get; set; }
        private long _lastSum;
        public long LastSum
        {
            get => _lastSum;
            set => SetProperty(ref _lastSum, value);
        }

        private long _newSum;
        public long NewSum
        {
            get => _newSum;
            set => SetProperty(ref _newSum, value);
        }

        private long _diff;
        public long Diff
        {
            get => _diff;
            set => SetProperty(ref _diff, value);
        }
        public void SetNewSum(long sum)
        {
            LastSum = NewSum;
            NewSum = sum;
            Diff = NewSum - LastSum;
        }
    }
}
