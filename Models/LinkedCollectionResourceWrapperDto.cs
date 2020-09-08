namespace Library.API.Models
{
    using System.Collections.Generic;

    public class LinkedCollectionResourceWrapperDto<T> : LinkedResourceBaseDto where T : LinkedResourceBaseDto
    {
        // ReSharper disable once MemberCanBePrivate.Global
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public IEnumerable<T> Value { get; }

        public LinkedCollectionResourceWrapperDto(IEnumerable<T> value)
        {
            Value = value;
        }
    }
}
