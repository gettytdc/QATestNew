using System.IdentityModel.Selectors;

namespace BluePrism.BPServer.ServerBehaviours
{
    internal class NullValidator : UserNamePasswordValidator
    {
        public override void Validate(string userName, string password)
        {
            return;
        }
    }
}
