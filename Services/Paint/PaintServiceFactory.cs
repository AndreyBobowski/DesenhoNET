namespace Services.Paint
{
    public class PaintServiceFactory : IPaintServiceFactory
    {
        public IPaintClient CreateClient(string connection, IReciver reciver)
        {
           return new PaintClient(connection, reciver);
        }

        public IPaintServer CreateServer(IReciver reciver)
        {
             return new PaintServer(reciver);
        }
    }

}