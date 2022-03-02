namespace BluePrism.Api.Models
{
    using System;
    using System.ComponentModel;
    using System.Text;
    using Func;
    using Newtonsoft.Json;

    [TypeDescriptionProvider(typeof(PagingTokenModelTypeDescriptionProvider))]
    public class PagingTokenModel<T>
    {
        private readonly PagingTokenState _pagingTokenState;

        public PagingTokenModel() => _pagingTokenState = PagingTokenState.Valid;
        private PagingTokenModel(PagingTokenState state) => _pagingTokenState = state;


        public T PreviousIdValue { get; set; }
        public string PreviousSortColumnValue { get; set; }
        public string DataType { get; set; }
        public string ParametersHashCode { get; set; }

        public PagingTokenState GetPagingTokenState() => _pagingTokenState;

        public static implicit operator PagingTokenModel<T>(string token) => ParsePagingToken<T>(token);
        public static implicit operator string(PagingTokenModel<T> model) => model.ToString();

        public override string ToString() =>
         this
             .Map(token => JsonConvert.SerializeObject(token, new PagingTokenModelJsonConverter<T>()))
             .Map(Encoding.UTF8.GetBytes)
             .Map(Convert.ToBase64String);

        private static PagingTokenModel<TIdType> ParsePagingToken<TIdType>(string token)
        {
            if (string.IsNullOrEmpty(token))
                return new PagingTokenModel<TIdType>(PagingTokenState.Empty);

            try
            {
                return token
                    .Map(Convert.FromBase64String)
                    .Map(Encoding.UTF8.GetString)
                    .Map(json => JsonConvert.DeserializeObject<PagingTokenModel<TIdType>>(json, new PagingTokenModelJsonConverter<TIdType>()));
            }
            catch
            {
                return new PagingTokenModel<TIdType>(PagingTokenState.Malformed);
            }
        }
    }
}
