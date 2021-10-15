using System.Threading.Tasks;

namespace Services.Paint
{
    public interface IReciver
    {
        Task Recive(string msg);
    }
}