using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Google.Protobuf;
using TeamNote.Protocol;

namespace TeamNote.Client
{
  class NetworkServices
  {
    public delegate void ServiceResponseDelegate(int type, ByteString message);

    private ServiceResponseDelegate m_ServiceResponse;

    public NetworkServices()
    {

    }

    public void RegisterServiceResponse(ServiceResponseDelegate serviceDelegate)
    {
      this.m_ServiceResponse = serviceDelegate;
    }

    public void Service_HandshakeResponse(HandshakeResponse serviceMessage)
    {

    }

  }
}
