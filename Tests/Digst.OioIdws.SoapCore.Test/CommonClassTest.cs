using Digst.OioIdws.CommonCore;
using Digst.OioIdws.CommonCore.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Digst.OioIdws.SoapCore.Test;

[TestClass]
public class CommonClassTest
{
    [TestMethod]
    public void TestCreateLogger()
    {
        // Arrange
        var logger = Logger.Instance;
        // Act
        
        // Assert
        Assert.IsNotNull(logger);
        logger.Fatal("test");
    }
}
