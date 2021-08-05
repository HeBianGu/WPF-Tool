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
    internal class UdpGroupViewModel : ClientViewModel
    {
        UdpClient client = null;

        #region - 属性 -

        private ClientParam _localParam = new ClientParam();
        /// <summary> 本地配置  </summary>
        public ClientParam LocalParam
        {
            get { return _localParam; }
            set
            {
                _localParam = value;
                RaisePropertyChanged("LocalParam");
            }
        }

        private int _count = 1;
        /// <summary> 说明  </summary>
        public int Count
        {
            get { return _count; }
            set
            {
                _count = value;
                RaisePropertyChanged("Count");
            }
        }

        #endregion

        #region - 命令 -

        #endregion

        #region - 方法 -

        protected override void Init()
        {
            this.LocalParam.Port = "7070";
            this.LocalParam.IP = "225.225.0.50";

            this.TargetParam.Port = "7070";
            this.TargetParam.IP = "225.225.0.50";
        }

        protected override void RelayMethod(object obj)
        {
            string command = obj.ToString();

            //  Do：启动接收端
            if (command == "Button.Click.ServerStart")
            {
                try
                { 
                    this.client = new UdpClient(Convert.ToInt32(this.LocalParam.Port));

                    this.client.JoinMulticastGroup(IPAddress.Parse(this.LocalParam.IP));

                    IPEndPoint nulticast = new IPEndPoint(IPAddress.Parse(this.LocalParam.IP), 0);


#pragma warning disable CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法
                    Task.Run(() =>
                    {
                        while (this.IsRunning)
                        {
                            try
                            {
                                var bytes = client.Receive(ref nulticast);

                                var request = this.Encoding.GetString(bytes, 0, bytes.Length);

                                this.AddMessage(nulticast.Address.ToString(), request, bytes);

                                //  Do ：接收到服务端关闭信息，关闭连接
                                if (request == "服务端已关闭")
                                {
                                    client.Close();
                                    this.IsRunning = false;
                                    return;
                                }
                                Thread.Sleep(10);
                            }
                            catch (Exception ex)
                            {

                            }
                        }
                    });
#pragma warning restore CS4014 // 由于此调用不会等待，因此在调用完成前将继续执行当前方法


                    this.IsRunning = true;
                }
                catch (Exception ex)
                {
                    MessageWindow.ShowSumit(ex.Message);
                }
            }
            //  Do：关闭本地监听
            else if (command == "Button.Click.ServerStop")
            {
                try
                {
                    if (this.IsRunning == false) return;

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


                    var bytes = this.EncodingSend.GetBytes(this.ClientText);

                    IPEndPoint point = new IPEndPoint(IPAddress.Parse(this.TargetParam.IP), Convert.ToInt32(this.TargetParam.Port));

                    UdpClient server = new UdpClient();

                    server.JoinMulticastGroup(point.Address);

                    for (int i = 0; i < this.Count; i++)
                    {
                        server.Send(bytes, bytes.Length, point);
                    }

                    server.Close();


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
