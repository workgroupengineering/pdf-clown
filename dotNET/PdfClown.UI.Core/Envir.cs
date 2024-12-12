using System.Threading;

namespace PdfClown.UI
{
    public static class Envir
    {
        public static void Init()
        {
            if (MainContext == null)
            {
                MainContext = SynchronizationContext.Current;
            }
        }

        public static SynchronizationContext? MainContext { get; set; }
    }

}

