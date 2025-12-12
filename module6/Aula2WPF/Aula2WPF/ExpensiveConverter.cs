using System;
using System.Globalization;
using System.Windows.Data;

namespace WpfXamlPerformanceDemo
{
    public class ExpensiveConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            // Converter caro que simula processamento
            if (value == null) return null;

            // Adicionar delay artificial
            if (parameter is int iterations)
            {
                // Cálculo pesado para simular overhead
                double result = 0;
                for (int i = 0; i < iterations; i++)
                {
                    result += Math.Sin(i) * Math.Cos(i);
                }

                return $"{value} (processed: {result:F4})";
            }

            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null) return null;

            try
            {
                string stringValue = value.ToString();
                
                // Se foi processado (contém " (processed: "), extrair o valor original
                if (stringValue.Contains(" (processed: "))
                {
                    int index = stringValue.IndexOf(" (processed: ");
                    stringValue = stringValue.Substring(0, index);
                }

                // Tentar converter para o tipo alvo
                if (targetType == typeof(string))
                {
                    return stringValue;
                }
                else if (targetType == typeof(int))
                {
                    if (int.TryParse(stringValue, out int intValue))
                        return intValue;
                }
                else if (targetType == typeof(double))
                {
                    if (double.TryParse(stringValue, out double doubleValue))
                        return doubleValue;
                }
                else if (targetType == typeof(float))
                {
                    if (float.TryParse(stringValue, out float floatValue))
                        return floatValue;
                }

                return stringValue;
            }
            catch
            {
                return value;
            }
        }
    }
}