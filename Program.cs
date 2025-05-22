using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

class Program
{
    static void Main()
    {
        // Зчитування параметрів з params.csv
        var parameters = ReadParameters("params.csv");
        string functionStr = parameters["function"];
        int N = int.Parse(parameters["N"]);
        double T = double.Parse(parameters["T"], CultureInfo.InvariantCulture);

        // Генерація значень t
        int numPoints = 1000;
        double[] tValues = new double[numPoints];
        double dt = T / (numPoints - 1);
        for (int i = 0; i < numPoints; i++)
            tValues[i] = i * dt;

        // Обчислення f(t)
        double[] fValues = new double[numPoints];
        for (int i = 0; i < numPoints; i++)
            fValues[i] = EvaluateFunction(functionStr, tValues[i]);

        // Обчислення коефіцієнтів Лагерра
        double[] coefficients = ComputeLaguerreCoefficients(fValues, tValues, N);

        // Відновлення функції
        double[] fRecValues = new double[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            double t = tValues[i];
            double sum = 0.0;
            for (int n = 0; n <= N; n++)
                sum += coefficients[n] * LaguerrePolynomial(n, t);
            fRecValues[i] = sum;
        }

        // Запис результатів у results.csv
        using (var writer = new StreamWriter("results.csv"))
        {
            writer.WriteLine("t,f_t,f_rec");
            for (int i = 0; i < numPoints; i++)
            {
                writer.WriteLine($"{tValues[i].ToString(CultureInfo.InvariantCulture)}," +
                                 $"{fValues[i].ToString(CultureInfo.InvariantCulture)}," +
                                 $"{fRecValues[i].ToString(CultureInfo.InvariantCulture)}");
            }
        }

        Console.WriteLine("✅ Результати збережено в results.csv");
    }

    static Dictionary<string, string> ReadParameters(string filePath)
    {
        var parameters = new Dictionary<string, string>();
        using (var reader = new StreamReader(filePath))
        {
            string headerLine = reader.ReadLine(); // function,N,T
            string valuesLine = reader.ReadLine(); // Math.Sin(t),10,10
            if (headerLine == null || valuesLine == null)
                throw new Exception("Файл параметрів порожній або пошкоджений.");

            string[] headers = headerLine.Split(',');
            string[] values = valuesLine.Split(',');

            for (int i = 0; i < headers.Length; i++)
                parameters[headers[i]] = values[i];
        }
        return parameters;
    }

    static double EvaluateFunction(string functionStr, double t)
    {
        return functionStr switch
        {
            "Math.Sin(t)" => Math.Sin(t),
            "Math.Exp(-t)" => Math.Exp(-t),
            _ => throw new NotImplementedException($"Функція '{functionStr}' не підтримується.")
        };
    }

    static double[] ComputeLaguerreCoefficients(double[] fValues, double[] tValues, int N)
    {
        int numPoints = tValues.Length;
        double[] coefficients = new double[N + 1];
        double dt = tValues[1] - tValues[0];

        for (int n = 0; n <= N; n++)
        {
            double sum = 0.0;
            for (int i = 0; i < numPoints; i++)
            {
                double t = tValues[i];
                double weight = Math.Exp(-t); // вага Лагерра
                sum += fValues[i] * LaguerrePolynomial(n, t) * weight * dt;
            }
            coefficients[n] = sum;
        }

        return coefficients;
    }

    static double LaguerrePolynomial(int n, double t)
    {
        if (n == 0) return 1.0;
        if (n == 1) return 1.0 - t;

        double L0 = 1.0;
        double L1 = 1.0 - t;
        for (int k = 2; k <= n; k++)
        {
            double Lk = ((2 * k - 1 - t) * L1 - (k - 1) * L0) / k;
            L0 = L1;
            L1 = Lk;
        }
        return L1;
    }
}