using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace WhosPetTests.Infrastructure
{
    [CollectionDefinition("Sequential-Tests")]
    public class SequentialTestsCollection : ICollectionFixture<TestFixture>
    {
       
    }

}
