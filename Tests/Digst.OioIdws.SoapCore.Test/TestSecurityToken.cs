using System.Collections.ObjectModel;
using System.IdentityModel.Tokens;
using System.Xml;

namespace Digst.OioIdws.SoapCore.Test;

public class TestSecurityToken : GenericXmlSecurityToken
{
    public TestSecurityToken() : base(new XmlDocument().CreateElement("empty"), null, DateTime.UtcNow, DateTime.UtcNow, null, null, null)
    {
    }

    public override string Id => throw new NotImplementedException();

    public override ReadOnlyCollection<SecurityKey> SecurityKeys => throw new NotImplementedException();

    public override DateTime ValidFrom => throw new NotImplementedException();

    public override DateTime ValidTo => throw new NotImplementedException();
}
