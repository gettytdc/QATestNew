using BluePrism.Core.Properties;
using System;
using System.Linq.Expressions;

namespace BluePrism.Core.Utility
{
    /// <summary>
    /// Utility functionality for working with type members using reflection
    /// </summary>
    public static class MemberAccessHelper
    {
        /// <summary>
        /// Gets the name of a property from a property access expression. This
        /// is limited to a single property. This uses the &quot;static reflection&quot;
        /// technique to avoid the hard-coding of property or field names.
        /// </summary>
        /// <typeparam name="TInstance">The type of the target instance</typeparam>
        /// <typeparam name="TMember">The type of the target member</typeparam>
        /// <param name="accessor">An expression representing access to a member expression</param>
        /// <returns></returns>
        public static string GetMemberName<TInstance,TMember>(Expression<Func<TInstance,TMember>> accessor)
        {
            var parameter = accessor.Parameters[0]; // The parameter x in expression x => x.MyProperty
            var memberExpression = accessor.Body as MemberExpression;
            if (memberExpression == null)
            {
                throw new ArgumentException(Resource.MemberAccessHelper_ExpressionMustSpecifyASimpleMemberAccessExpression, nameof(accessor));
            }
            // Check that we are referencing member belonging to x
            var target = memberExpression.Expression as ParameterExpression;
            if(target == null || target != parameter)
            {
                throw new ArgumentException(Resource.MemberAccessHelper_ExpressionMustSpecifyASimpleMemberOnTheTargetInstanceNestedMembersAreNotPermitt, nameof(accessor));
            }
            return memberExpression.Member.Name;
        }
        
    }
}