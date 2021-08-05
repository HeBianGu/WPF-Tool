using HeBianGu.Base.WpfBase;
using HeBianGu.General.WpfControlLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace HeBianGu.App.SocketTool
{
    class ShellViewModel : NotifyPropertyChanged
    {
        private ServerViewModel _server = new ServerViewModel();
        /// <summary> 说明  </summary>
        public ServerViewModel Server
        {
            get { return _server; }
            set
            {
                _server = value;
                RaisePropertyChanged("Server");
            }
        }

        private ClientViewModel _client = new ClientViewModel();
        /// <summary> 说明  </summary>
        public ClientViewModel Client
        {
            get { return _client; }
            set
            {
                _client = value;
                RaisePropertyChanged("Client");
            }
        }



        private UdpClientViewModel _udpClient=new UdpClientViewModel();
        /// <summary> 说明  </summary>
        public UdpClientViewModel UdpClient
        {
            get { return _udpClient; }
            set
            {
                _udpClient = value;
                RaisePropertyChanged("UdpClient");
            }
        }



        private UdpGroupViewModel _udpGroupClient = new UdpGroupViewModel();
        /// <summary> 说明  </summary>
        public UdpGroupViewModel UdpGroupClient
        {
            get { return _udpGroupClient; }
            set
            {
                _udpGroupClient = value;
                RaisePropertyChanged("UdpGroupClient");
            }
        }

        

        private ObservableCollection<Encoding> _encoding = new ObservableCollection<Encoding>();
        /// <summary> 说明  </summary>
        public ObservableCollection<Encoding> Encodings
        {
            get { return _encoding; }
            set
            {
                _encoding = value;
                RaisePropertyChanged("Encodings");
            }
        }


        protected override void Init()
        {
            this.Encodings.Clear();
            this.Encodings.Add(Encoding.Default);
            this.Encodings.Add(Encoding.ASCII);
            this.Encodings.Add(Encoding.UTF7);
            this.Encodings.Add(Encoding.UTF8);
            this.Encodings.Add(Encoding.ASCII);
            this.Encodings.Add(Encoding.UTF32);
            this.Encodings.Add(Encoding.Unicode);
            this.Encodings.Add(Encoding.BigEndianUnicode);
            this.Encodings.Add(Encoding.GetEncoding("GB2312"));
        }

        /// <summary> 命令通用方法 </summary>
        protected override async void RelayMethod(object obj)
        {
            string command = obj?.ToString();

        }
    }


    class ServerParam
    {
        [Display(Name = "端口号")]
        [Required]
        [Range(0, 100000)]
        public string Port { get; set; } = "7777";
    }

    class ClientParam : ServerParam
    {
        [Display(Name = "IP地址")]
        [Required]
        [RegularExpression(@"(([01]{0,1}\d{0,1}\d|2[0-4]\d|25[0-5])\.){3}([01]{0,1}\d{0,1}\d|2[0-4]\d|25[0-5])", ErrorMessage = "IP不合法")]
        public string IP { get; set; } = "127.0.0.1";
    }

    /// <summary> 说明</summary>
    internal class Message : NotifyPropertyChanged
    {
        #region - 属性 -
        [Browsable(false)]
        public int Type { get; set; }

        [Display(Name = "时间")]
        public string Time { get; set; }

        [Display(Name = "数据")]
        public string Data { get; set; }

        [Display(Name = "标题")]
        public string Title { get; set; }

        [Display(Name = "Byte值")]
        public string Tip { get; set; }

        [Display(Name = "Bytes")]
        public byte[] Bytes { get; set; }

        #endregion

        #region - 命令 -

        #endregion

        #region - 方法 -

        protected override async void RelayMethod(object obj)
        {
            string command = obj.ToString();

            //  Do：应用
            if (command == "Button.Click.ShowDetial")
            {
                await MessageService.ShowObjectWithContent(this);

            }
            //  Do：取消
            else if (command == "Cancel")
            {


            }
        }

        #endregion
    }

}
