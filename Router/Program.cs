
namespace Router
{
    class Program
    {
        static void Main(string[] args)
        {
            Router router = new Router(args[0], args[1]);
            router.Start(); 
         }
    }
}
