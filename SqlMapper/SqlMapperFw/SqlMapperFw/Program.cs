using System;
using System.Collections.Generic;
using LinFu.DynamicProxy;
using SqlMapperFw.BuildMapper;
using SqlMapperFw.DataMappers;
using SqlMapperFw.Entities;

namespace SqlMapperFw
{

    internal class MyInterceptor : IInvokeWrapper
    {
        public void BeforeInvoke(InvocationInfo info)
        {
        }

        public object DoInvoke(InvocationInfo info)
        {
            Console.Write(info.TargetMethod.Name + " invoked with args: ");
            foreach (var a in info.Arguments)
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
        static void Main()
        {

            ProxyFactory factory = new ProxyFactory();
            IDataMapper<Product> proxy = factory.CreateProxy<IDataMapper<Product>>(new MyInterceptor());
            proxy.Add(5, 7);
            proxy.Mul(3, 11);
            proxy.Mul(9, 8);
            proxy.Div(9, 8);
            Product p = new Product();
            proxy.Insert(p);
            proxy.Update(p);
            proxy.Delete(p);

            //Builder b = new Builder(Product.class, ...);

            //Builder b = new Builder(..., ...);

            //IDataMapper<Order> orderMapper = b.Build<Order>(); //1ªparte 1.
            //IDataMapper<Customer> custMapper = b.Build<Customer>(); //1ªparte 1.
            //IDataMapper<Employee> empMapper = b.Build<Employee>(); //1ªparte 1.


            //IDataMapper<Product> prodMapper = b.Build<Product>(); //1ªparte 1.
            //IEnumerable<Product> prods = prodMapper.GetAll();
            //prods.Where("CategoryID = 7").Where("UnitsinStock > 30"); //1ªparte 2.
        }
    }
}
