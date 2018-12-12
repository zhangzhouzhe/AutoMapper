using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace AutoMapper
{
    public class Class1
    {
        [Fact]
        public void CreateExpression()
        {
            LabelTarget returnTarget = Expression.Label();
            BlockExpression bolckExpr =
                Expression.Block(
                    Expression.Call(
                        typeof(Console).GetMethod("WriteLine", new Type[] { typeof(string) }), Expression.Constant("GoTo")),
                    Expression.Label(returnTarget));
            Expression.Lambda<Action>(bolckExpr).Compile()();
                   
        }


        [Fact]
        public void Test()
        {
            var person = new Person()
            {
                Age = 26,
                Email = "474726262@qq.com",
                Name = "zzz",
                UserId = 666
            };

            var funcMap = Map<Person, Staff>();
            var staff = funcMap(person);
        }
        [Fact]
        public void AccoutDtoTest()
        {
            var dto = new AccountDto()
            {
                BindingMobile = "13456323232",
                UserId = 100345124,
                UserName = "151442154"
            };
            var record = new LoginNameRecord();
            dto.GetLoginNamesFromRecord(record);
        }


        static Func<TSource, TTarget> Map<TSource, TTarget>()
        {
            var sourceType = typeof(TSource);
            var targetType = typeof(TTarget);

            var parameterExpression = Expression.Parameter(sourceType, "p");

            var memberBindingList = new List<MemberBinding>();
            foreach (var sourceItem in sourceType.GetProperties())
            {
                var targetItem = targetType.GetProperty(sourceItem.Name);
                if (targetItem == null || sourceItem.PropertyType != targetItem.PropertyType)
                    continue;
                var property = Expression.Property(parameterExpression, sourceItem);
                var memberBinding = Expression.Bind(targetItem, property);
                memberBindingList.Add(memberBinding);
            }

            var memberInitExpression = Expression.MemberInit(Expression.New(targetType), memberBindingList);
            var lambda = Expression.Lambda<Func<TSource, TTarget>>(memberInitExpression, parameterExpression);

            return lambda.Compile();
        }
        private static Action<Dictionary<string, object>, ObjectData> ObjectMapper()
        {
            var ObjectDataProps = typeof(ObjectData)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(p => p.Name).ToArray();

            var retLabel = Expression.Label(typeof(void));
            var dicExpr = Expression.Parameter(typeof(Dictionary<string, object>));
            var objExpr = Expression.Parameter(typeof(ObjectData));
  
        }
        private static Action<AccountDto, LoginNameRecord> GenerateLoginNamesMapper()
        {
            var loginNameProps = typeof(AccountDto)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(p => p.Name)
                .Intersect(Enum.GetNames(typeof(LoginNameType)), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var retLabel = Expression.Label(typeof(void));
            var dtoExpr = Expression.Parameter(typeof(AccountDto));
            var recordExpr = Expression.Parameter(typeof(LoginNameRecord));
            var bodyExpr = Expression.Switch(Expression.Property(recordExpr, "LoginNameType"),
                loginNameProps.Select(prop =>
                    Expression.SwitchCase(
                        Expression.Block(
                            Expression.Assign(Expression.Property(dtoExpr, prop),
                                Expression.Property(recordExpr, "LoginName")),
                            Expression.Return(retLabel)),
                    Expression.Constant((LoginNameType)Enum.Parse(typeof(LoginNameType), prop)))).ToArray());

            return Expression.Lambda<Action<AccountDto, LoginNameRecord>>(
                Expression.Block(bodyExpr, Expression.Label(retLabel)),
                new[] { dtoExpr, recordExpr }).Compile();
        }

    }

    internal static class AccountDtoExtension
    {
        public static void GetLoginNamesFromRecord(this AccountDto dto, LoginNameRecord record)
        {
            _loginNamesMapper.Value(dto, record);
        }

        private static Lazy<Action<AccountDto, LoginNameRecord>> _loginNamesMapper
            = new Lazy<Action<AccountDto, LoginNameRecord>>(GenerateLoginNamesMapper, true);

        private static Action<AccountDto, LoginNameRecord> GenerateLoginNamesMapper()
        {
            var loginNameProps = typeof(AccountDto)
                .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Select(p => p.Name)
                .Intersect(Enum.GetNames(typeof(LoginNameType)), StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var retLabel = Expression.Label(typeof(void));
            var dtoExpr = Expression.Parameter(typeof(AccountDto));
            var recordExpr = Expression.Parameter(typeof(LoginNameRecord));
            var bodyExpr = Expression.Switch(Expression.Property(recordExpr, "LoginNameType"),
                loginNameProps.Select(prop =>
                    Expression.SwitchCase(
                        Expression.Block(
                            Expression.Assign(Expression.Property(dtoExpr, prop),
                                Expression.Property(recordExpr, "LoginName")),
                            Expression.Return(retLabel)),
                    Expression.Constant((LoginNameType)Enum.Parse(typeof(LoginNameType), prop)))).ToArray());

            return Expression.Lambda<Action<AccountDto, LoginNameRecord>>(
                Expression.Block(bodyExpr, Expression.Label(retLabel)),
                new[] { dtoExpr, recordExpr }).Compile();
        }
    }
    public class DataMapper<TSource, TTarget>
    {
        private static Func<TSource,TTarget> MapFunc { get; set; }

   
    }
    public class Person
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public string Age { get; set; }
        public DateTime CreateTime { get; set; }
    }
    public class ObjectData
    {
        public int TenantId { get; set; }
        public string Age { get; set; }
        public DateTime CreateTime { get; set; }
        public Dictionary<string,object> MetaFields { get; set; }
    }
    [Serializable]
    internal class LoginNameRecord
    {
        public int UserId { get; set; }
        public string LoginName { get; set; }
        public LoginNameType LoginNameType { get; set; }
    }
    public enum LoginNameType
    {
        /// <summary>  
        /// 用户名  
        /// </summary>
        UserName = 0,

        /// <summary>  
        /// 绑定手机  
        /// </summary> 
        BindingMobile = 1,

        /// <summary>  
        /// 绑定邮箱  
        /// </summary>
        [Obsolete("暂未使用")]
        BindingEmail = 2
    }
    public class AccountDto
    {

        public int UserId { get; set; }

        //以下登录名名称需要与LoginNameType枚举值相同，详见LoginNameRecord.cs

        public string UserName { get; set; }


        public string BindingMobile { get; set; }
    }
    public class Staff
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public int UserId { get; set; }
        public int Age { get; set; }
    }
}
