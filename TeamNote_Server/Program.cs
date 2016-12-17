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
    static void Main(string[] args)
    {
      int a = 0;


      ServerInstance l_serverInstance = new ServerInstance();
      Debug.Setup("test.log");
      Debug.Log("Test");

      try {
        a = 3 / a;
      }
      catch (Exception e) {
        Debug.Exception(e);
      }

      Console.ReadKey();

    }
  }
}
