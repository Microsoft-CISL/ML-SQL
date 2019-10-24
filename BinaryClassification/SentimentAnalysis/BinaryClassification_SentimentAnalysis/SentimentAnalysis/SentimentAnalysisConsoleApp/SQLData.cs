using System;
namespace HeartDiseaseDetection
{
    public interface SQLData
    {

        string GetHeader(string separator);


        string getData(string separator);

        string getSQLData(string separator);
        
    }
}
