
using ConsoleApplication1.Base;

namespace ConsoleApplication1.DataAccess.FileReaders
{
    public interface IXmlReader
    {
        AllConfig GetObject(string fileName);
    }
}