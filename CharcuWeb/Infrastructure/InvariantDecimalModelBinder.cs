using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TradicioCarnica.Infrastructure
{
    public class InvariantDecimalModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder? GetBinder(ModelBinderProviderContext context) =>
            context.Metadata.UnderlyingOrModelType == typeof(decimal)
                ? new InvariantDecimalModelBinder()
                : null;
    }

    public class InvariantDecimalModelBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext ctx)
        {
            var vr = ctx.ValueProvider.GetValue(ctx.ModelName);
            if (vr == ValueProviderResult.None) return Task.CompletedTask;

            ctx.ModelState.SetModelValue(ctx.ModelName, vr);
            var s = vr.FirstValue?.Trim();

            if (string.IsNullOrEmpty(s))
            {
                if (ctx.ModelMetadata.IsNullableValueType)
                    ctx.Result = ModelBindingResult.Success(null);
                else
                    ctx.ModelState.TryAddModelError(ctx.ModelName, "Indica un número.");
                return Task.CompletedTask;
            }

            // Acepta tanto "1.5" (input type=number) como "1,5" (escrito a mano)
            if (decimal.TryParse(s.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var d))
                ctx.Result = ModelBindingResult.Success(d);
            else
                ctx.ModelState.TryAddModelError(ctx.ModelName, "Número no válido.");

            return Task.CompletedTask;
        }
    }
}