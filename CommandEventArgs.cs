using System;
using BarcodeAppDesktop.Enumrations;
using GalaSoft.MvvmLight;

namespace BarcodeAppDesktop.Helper
{
    public class CommandEventArgs : EventArgs
    {
        public CommandEventArgs(CrossViewModelEventType type, ViewModelBase viewModel)
        {
            EventType = type;
            SourceViewModel = viewModel;
        }

        public CommandEventArgs(CrossViewModelEventType type, ViewModelBase viewModel, Object additionalObj)
        {
            EventType = type;
            SourceViewModel = viewModel;
            AdditionalObj = additionalObj;
        }

        public CrossViewModelEventType EventType { get; set; }
        public ViewModelBase SourceViewModel { get; set; }
        public Object AdditionalObj { get; set; }
    }
}