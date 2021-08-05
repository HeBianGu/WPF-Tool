using HeBianGu.Base.WpfBase;
using HeBianGu.General.WpfControlLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace HeBianGu.App.SocketTool
{

    /// <summary> 说明</summary>
    internal class ClientViewModel : NotifyPropertyChanged
    {
        #region - 属性 -
        private ClientParam _targetParam = new ClientParam();
        /// <summary> 目标配置  </summary>
        public ClientParam TargetParam
        {
            get { return _targetParam; }
            set
            {
                _targetParam = value;
                RaisePropertyChanged("TargetParam");
            }
        }

        private ObservableCollection<Message> _clientMessages = new ObservableCollection<Message>();
        /// <summary> 说明  </summary>
        public ObservableCollection<Message> ClientMessages
        {
            get { return _clientMessages; }
            set
            {
                _clientMessages = value;
                RaisePropertyChanged("ClientMessages");
            }
        }


        private string _clientText;
        /// <summary> 说明  </summary>
        public string ClientText
        {
            get { return _clientText; }
            set
            {
                _clientText = value;
                RaisePropertyChanged("ClientText");
            }
        }

        private bool _isRunning;
        /// <summary> 说明  </summary>
        public bool IsRunning
        {
            get { return _isRunning; }
            set
            {
                _isRunning = value;
                RaisePropertyChanged("IsRunning");
            }
        }

        private Encoding _encoding = Encoding.UTF8;
        /// <summary> 说明  </summary>
        public Encoding Encoding
        {
            get { return _encoding; }
            set
            {
                _encoding = value;
                RaisePropertyChanged("Encoding");
            }
        }


        private Encoding _encodingSend = Encoding.UTF8;
        /// <summary> 说明  </summary>
        public Encoding EncodingSend
        {
            get { return _encodingSend; }
            set
            {
                _encodingSend = value;
                RaisePropertyChanged("EncodingSend");
            }
        }

        private int _packageCount = 1024;
        /// <summary> 说明  </summary>
        public int PackageCount
        {
            get { return _packageCount; }
            set
            {
                _packageCount = value;
                RaisePropertyChanged("PackageCount");
            }
        }

        private int _spanTime = 100;
        /// <summary> 时间间隔  </summary>
        public int SpanTime
        {
            get { return _spanTime; }
            set
            {
                _spanTime = value;
                RaisePropertyChanged("SpanTime");
            }
        }


        Socket client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);


        #endregion

        #region - 命令 -

        #endregion

        #region - 方法 -

        protected override async void RelayMethod(object obj)
        {
            string command = obj.ToString();

            //  Do：应用
            if (command == "Sumit")
            {


            }
            //  Do：取消
            else if (command == "Cancel")
            {


            }
            //  Do ：客户端连接到服务器
            else if (command == "Button.Click.ClientStart")
            {
                try
                {
                    client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                    client.Connect(new IPEndPoint(IPAddress.Parse(this.TargetParam.IP), Convert.ToInt32(this.TargetParam.Port)));

#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(() =>
                    {
                        while (client.Connected)
                        {
                            try
                            {
                                var bytes = new byte[1000];

                                var len = client.Receive(bytes);

                                var request = this.Encoding.GetString(bytes, 0, len);

                                //  Do ：接收到服务端关闭信息，关闭连接
                                if (request == "服务端已关闭")
                                {
                                    client.Close();
                                    this.IsRunning = false;
                                }

                                this.AddMessage(this.TargetParam.IP, request, bytes);

                                Thread.Sleep(10);
                            }
                            catch (Exception ex)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageService.ShowWinErrorMessage(ex.Message);
                                });
                            }
                        }
                    });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法

                    this.IsRunning = true;

                    MessageService.ShowSnackMessageWithNotice("客户端连接成功");
                }
                catch (Exception ex)
                {
                    MessageWindow.ShowSumit(ex.Message);
                }


            }
            //  Do ：客户端断开连接
            else if (command == "Button.Click.ClientStop")
            {
                try
                {
                    client?.Close();

                    this.IsRunning = false;

                    MessageService.ShowSnackMessageWithNotice("操作成功");
                }
                catch (Exception ex)
                {
                    MessageWindow.ShowSumit("操作失败!" + ex.Message);
                }
            }

            //  Do ：客户端向服务器发数据
            else if (command == "Button.Click.ClientSend")
            {
                try
                {
                    if (string.IsNullOrEmpty(this.ClientText))
                    {
                        MessageService.ShowSnackMessageWithNotice("传入数据不能为空");
                        return;
                    }

                    //await Task.Run(()=>
                    // {
                    //     var data = this.EncodingSend.GetBytes(this.ClientText);

                    //     client.Send(data);
                    // });

                    await MessageService.ShowWinProgressBarMessage(l =>
                     {
                         var data = this.EncodingSend.GetBytes(this.ClientText);

                         //  Do ：拆包发送
                         int index = 0;

                         int count = this.PackageCount;

                         double total = Math.Ceiling((data.Length / (count * 1.0)));

                         while (true)
                         {
                             var c = data.Skip(index * count).Take(count).ToArray();

                             client.Send(c);

                             Application.Current.Dispatcher.Invoke(() =>
                             {
                                 l.Value = 100.0 * index / total;
                             });


                             if (c.Length < count) break;

                             index++;

                             Thread.Sleep(this.SpanTime);
                         }

                         return true;
                     });

                    MessageService.ShowSnackMessageWithNotice("操作成功");
                }
                catch (Exception ex)
                {
                    MessageWindow.ShowSumit("操作失败!" + ex.Message);
                }
            }
        }

        public void AddMessage(string title, string message, byte[] bytes)
        {
            Message m = new Message();
            m.Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            m.Title = title;
            m.Data = message;

            if (bytes != null)
            {
                List<byte> bs = bytes.ToList();

                for (int i = bytes.Length - 1; i >= 0; i--)
                {
                    if (bytes[i] == 0x00)
                    {
                        bs.RemoveAt(i);
                    }
                    else
                    {
                        break;
                    }
                }

                m.Bytes = bs.ToArray();
                m.Tip = BitConverter.ToString(m.Bytes);
            }



            Application.Current.Dispatcher.Invoke(() =>
            {
                this.ClientMessages.Insert(0, m);
            });

        }
        #endregion
    }
}
