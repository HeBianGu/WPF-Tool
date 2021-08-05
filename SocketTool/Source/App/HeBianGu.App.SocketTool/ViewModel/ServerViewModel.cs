using HeBianGu.Base.WpfBase;
using HeBianGu.General.WpfControlLib;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HeBianGu.App.SocketTool
{
    /// <summary> 说明</summary>
    internal class ServerViewModel : NotifyPropertyChanged
    {


        Socket server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

        #region - 属性 -

        private ServerParam _serverParam = new ServerParam();
        /// <summary> 说明  </summary>
        public ServerParam ServerParam
        {
            get { return _serverParam; }
            set
            {
                _serverParam = value;
                RaisePropertyChanged("ServerParam");
            }
        }

        private ObservableCollection<Message> _serverMessage = new ObservableCollection<Message>();
        /// <summary> 说明  </summary>
        public ObservableCollection<Message> ServerMessages
        {
            get { return _serverMessage; }
            set
            {
                _serverMessage = value;
                RaisePropertyChanged("ServerMessages");
            }
        }


        private string _serverText;
        /// <summary> 说明  </summary>
        public string ServerText
        {
            get { return _serverText; }
            set
            {
                _serverText = value;
                RaisePropertyChanged("ServerText");
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

        private int _selectedIndex = 0;
        /// <summary> 说明  </summary>
        public int SelectedIndex
        {
            get { return _selectedIndex; }
            set
            {
                _selectedIndex = value;
                RaisePropertyChanged("SelectedIndex");
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

        #endregion

        #region - 命令 -

        #endregion

        #region - 方法 -

        protected override void Init()
        {
            this.ConnectClients.CollectionChanged += (l, k) =>
              {
                  this.SelectedClient = this.ConnectClients?.FirstOrDefault();
              };

        }

        protected override void RelayMethod(object obj)
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
            //  Do ：服务端向客户端发数据
            else if (command == "Button.Click.ServerSend")
            {
                if (this.server == null)
                {
                    MessageService.ShowSnackMessageWithNotice("服务端没有启动监听");
                    return;
                }

                try
                {
                    Socket client = this.SelectedClient;

                    if (client == null)
                    {
                        MessageService.ShowSnackMessageWithNotice("没有监测到连接的客户端");
                        return;
                    }

                    if (string.IsNullOrEmpty(this.ServerText))
                    {
                        MessageService.ShowSnackMessageWithNotice("传入数据不能为空");
                        return;
                    }

                    //  Do ：发送到客户端数据
                    var bs = this.EncodingSend.GetBytes(this.ServerText);

                    //var sss = Convert.ToInt32(this.ServerText);

                    //bs = BitConverter.GetBytes(sss);

                    client.Send(bs);

                    MessageService.ShowSnackMessageWithNotice("操作成功");
                }
                catch (Exception ex)
                {
                    MessageWindow.ShowSumit("操作失败!" + ex.Message);
                }
            }

            //  Do ：服务端开始监听
            else if (command == "Button.Click.ServerStart")
            {
                server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                //  Do ：初始化服务端
                server.Bind(new IPEndPoint(IPAddress.Any, Convert.ToInt32(this.ServerParam.Port)));

                server.Listen(1000);

                //  Do ：循环接收客户端连接
                server.BeginAccept(Accept, server);

                MessageService.ShowSnackMessageWithNotice("服务端启动成功");

                this.IsRunning = true;
            }
            //  Do ：服务器停止监听
            else if (command == "Button.Click.ServerStop")
            {
                try
                {
                    this.IsRunning = false;

                    server?.Close();

                    //  Do ：关闭当前连接的客户端
                    foreach (var client in ConnectClients)
                    {
                        client.Send(this.EncodingSend.GetBytes("服务端已关闭"));
                        client.Close();
                    }

                    //  Do ：有客户端连接放入缓存中  
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        ConnectClients.Clear();
                    });

                    MessageService.ShowSnackMessageWithNotice("操作成功");
                }
                catch (Exception ex)
                {


                }
            }
        }



        public void Accept(IAsyncResult l)
        {
            if (this.IsRunning == false) return;

            Socket serverSocket = l.AsyncState as Socket;

            Socket clientSocket = serverSocket.EndAccept(l);

            var ipe = (IPEndPoint)clientSocket.RemoteEndPoint;

            string ip = ipe?.Address.ToString();

            this.AddMessage(ip, $"客户端已连接[{ip}]");

            clientSocket.Send(this.EncodingSend.GetBytes("服务器连接成功"));

            //  Do ：有客户端连接放入缓存中  
            Application.Current.Dispatcher.Invoke(() =>
            {
                ConnectClients.Add(clientSocket);
            });

            serverSocket.BeginAccept(Accept, serverSocket);

            //  Do ：接收到连接时，循环接收当前客户端的数据
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Revice, clientSocket);


        }

        private ObservableCollection<Socket> _connectClients = new ObservableCollection<Socket>();
        /// <summary> 当前连接的客户端  </summary>
        public ObservableCollection<Socket> ConnectClients
        {
            get { return _connectClients; }
            set
            {
                _connectClients = value;
                RaisePropertyChanged("ConnectClients");
            }
        }


        private Socket _selectedClient;
        /// <summary> 说明  </summary>
        public Socket SelectedClient
        {
            get { return _selectedClient; }
            set
            {
                _selectedClient = value;
                RaisePropertyChanged("SelectedClient");
            }
        }


        byte[] buffer = new byte[1024*1024];

        public void AddMessage(string title, string message, byte[] bytes = null)
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
                this.ServerMessages.Insert(0, m);
            });

        }

        public void Revice(IAsyncResult l)
        {
            Socket clientSocket = l.AsyncState as Socket;

            ////  Do ：停止服务，停止循环接收数据
            //if (this.IsRunning == false) return;

            if (clientSocket.Connected == false) return;

            var state = clientSocket.Poll(5000, SelectMode.SelectRead);

            //  Do ：如果客户端断开连接返回的消息，则删除当前客户端
            if (state == true)
            {
                Application.Current.Dispatcher.Invoke(() =>
                {
                    this.ConnectClients.Remove(clientSocket);
                });

                return;
            }

            clientSocket.EndReceive(l);

            l.AsyncWaitHandle.Close();

            clientSocket.Send(this.EncodingSend.GetBytes("服务器接收成功"));

            var ipe = (IPEndPoint)clientSocket.RemoteEndPoint;

            string ip = ipe?.Address.ToString();


            //  Do ：服务端接收到消息 
            var request = this.Encoding.GetString(buffer)?.TrimEnd('\0');

            this.AddMessage(ip, request, buffer);

            buffer = new byte[buffer.Length];
            clientSocket.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, Revice, clientSocket);
        }

        #endregion
    }


}
