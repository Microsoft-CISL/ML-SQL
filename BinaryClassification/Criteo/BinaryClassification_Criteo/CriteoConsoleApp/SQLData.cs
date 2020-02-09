using System;
namespace Regression_TaxiFarePrediction
{
    public interface SQLData
    {

        string GetHeader(string separator);


        string getData(string separator);

        string getSQLData(string separator);
        
    }
}
