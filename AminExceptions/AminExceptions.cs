using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AminExceptions
{
    public static class CAminExceptions
    {
        //private static const string baseMessage = "Skontaktuj się z Głównym Programistą i przekaż mu:";

        public static void ThrowException(Exception e, string messageToShow)
        {
            StringBuilder sb = new StringBuilder();
            
           // sb.AppendLine(baseMessage);
            sb.AppendLine("Skontaktuj się z Głównym Programistą i przekaż mu:");
            sb.AppendLine(messageToShow);
            if (e != null)
            {
                sb.AppendLine(e.Message);
            }

            throw new Exception(sb.ToString());
        }
    }
}
