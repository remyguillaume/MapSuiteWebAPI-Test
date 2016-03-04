using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Web.Http;
using ThinkGeo.MapSuite.Core;

namespace GettingStarted.Controllers
{
    [RoutePrefix("Feature")]
    public class FeatureController : ApiController
    {
        [Route("{layerId}/all")]
        [HttpGet]
        public HttpResponseMessage QueryAnlage(string layerId)
        {
            string shapeFileName = GetFullPath(@"Data\Anlage\Anlage.shp");
            var featureLayer = new ShapeFileFeatureLayer(shapeFileName);

            featureLayer.Open();
            Collection<Feature> allFeatures = featureLayer.QueryTools.GetAllFeatures(ReturningColumnsType.AllColumns);
            featureLayer.Close();

            var str = GetGeoJSon(allFeatures);

            var responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
            responseMessage.Content = new StringContent(str.ToString());
            responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            return responseMessage;
            
        }

        const string JSonHeader = @"{""type"": ""FeatureCollection"",""crs"": {""type"": ""name"",""properties"": {""name"": ""EPSG:3857""}},""features"": [";
        private static StringBuilder GetGeoJSon(Collection<Feature> allFeatures)
        {
            var str = new StringBuilder(JSonHeader);
            foreach (Feature feature in allFeatures)
                str.Append(feature.GetGeoJson() + ",");
            str.Remove(str.Length - 1, 1).Append("]}");
            return str;
        }

        private static string GetFullPath(string relativePath)
        {
            var uri = new Uri(Assembly.GetExecutingAssembly().CodeBase);
            string folderPath = Path.GetDirectoryName(Path.GetDirectoryName(uri.LocalPath));
            return Path.Combine(folderPath, relativePath);
        }
    }
}