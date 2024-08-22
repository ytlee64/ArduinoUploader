using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArduinoUploader
{
    partial class MainVM : ObservableObject
    {
        [ObservableProperty]
        public string  _outputText="";

        public MainVM()
        {
            WeakReferenceMessenger.Default.Register<string>(this, Update);
        }

        private void Update(object recipient, string msg)
        {
            OutputText += msg+Environment.NewLine;
        }
    }
}
