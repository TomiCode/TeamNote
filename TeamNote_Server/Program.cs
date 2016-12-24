using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TeamNote.Server;

namespace TeamNote
{
  class Program
  {
    private static ServerInstance serverInstance;
    
    static void Main(string[] args)
    {
      Debug.Setup("test.log");
      serverInstance = new ServerInstance();
      serverInstance.Start();

      Console.ReadKey();
    }
  }
}
