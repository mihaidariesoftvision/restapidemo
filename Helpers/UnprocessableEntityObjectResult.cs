//namespace Library.API.Helpers
//{
//    using System;
//    using Microsoft.AspNetCore.Mvc.ModelBinding;
//    using Microsoft.AspNetCore.Mvc;

//    public class UnprocessableEntityObjectResult : ObjectResult
//    {
//        public UnprocessableEntityObjectResult(ModelStateDictionary modelState) : base(new SerializableError(modelState))
//        {
//            if (modelState == null)
//            {
//                throw new ArgumentNullException(nameof(modelState));
//            }

//            StatusCode = 422;
//        }
//    }
//}
