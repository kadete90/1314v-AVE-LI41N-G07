using LinFu.DynamicProxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

internal class MyInterceptor : IInvokeWrapper
{
    public void BeforeInvoke(InvocationInfo info)
    {
    }

    public object DoInvoke(InvocationInfo info)
    {
        Console.Write(info.TargetMethod.Name + " invoked with args: ");
        foreach(var a in info.Arguments)
        {
            Console.Write(a + " ");
        }
        Console.WriteLine();
        return 0;
    }

    public void AfterInvoke(InvocationInfo info, object returnValue)
    {
    }
}

public interface IMyInterface
{
    int Add(int x, int y);
    int Mul(int x, int y);
    int Div(int x, int y);
}

class Program
{
    static void Main(string[] args)
    {
        ProxyFactory factory = new ProxyFactory();
        IMyInterface proxy = factory.CreateProxy<IMyInterface>(new MyInterceptor());
        proxy.Add(5, 7);
        proxy.Mul(3, 11);
        proxy.Mul(9, 8);
        proxy.Div(9, 8);
    }
}
