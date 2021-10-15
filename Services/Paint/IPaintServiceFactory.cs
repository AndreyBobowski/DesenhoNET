namespace Services.Paint
{
    public interface IPaintServiceFactory
    {
        IPaintServer CreateServer(IReciver reciver);
        IPaintClient CreateClient(string connection, IReciver reciver);
    }
}