﻿using System;
using System.IO.Pipes;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Ipc
{
    public class IpcClientService
    {
        internal IpcClientService(IpcContext ipcContext, string serverName = IpcContext.PipeName)
        {
            IpcContext = ipcContext;
            ServerName = serverName;
        }

        public async Task Start()
        {
            var namedPipeClientStream = new NamedPipeClientStream(".", ServerName, PipeDirection.InOut,
                PipeOptions.None, TokenImpersonationLevel.Impersonation);
            await namedPipeClientStream.ConnectAsync();

            NamedPipeClientStream = namedPipeClientStream;
        }

        public void Stop()
        {
            // 告诉服务器端不连接
        }

        private NamedPipeClientStream NamedPipeClientStream { set; get; } = null!;

        public Task WriteStringAsync(string text)
        {
            var buffer = Encoding.UTF8.GetBytes(text);
            return WriteMessageAsync(buffer, 0, buffer.Length);
        }

        public async Task WriteMessageAsync(byte[] buffer, int offset, int count)
        {
            var ack = AckManager.GetAck();
            await IpcMessageConverter.WriteAsync(NamedPipeClientStream, IpcConfiguration.MessageHeader, ack, buffer, offset,
                  count);
            await NamedPipeClientStream.FlushAsync();
        }

        internal AckManager AckManager => IpcContext.AckManager;

        private IpcConfiguration IpcConfiguration => IpcContext.IpcConfiguration;

        public IpcContext IpcContext { get; }
        public string ServerName { get; }
    }
}