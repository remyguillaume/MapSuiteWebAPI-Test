using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;
using ThinkGeo.MapSuite.Core;
using ThinkGeo.MapSuite.WebApiEdition;

namespace GettingStarted.Controllers
{
    [RoutePrefix("Tile")]
    public class TileController : ApiController
    {
        [Route("{layerId}/{z}/{x}/{y}")]
        [HttpGet]
        public HttpResponseMessage GetAnlage(string layerId, int z, int x, int y)
        {
            Debug.WriteLine(z + " ; " + x + " ; " + y);
            string shapeFileName = GetFullPath(@"Data\Anlage\Anlage.shp");
            var featureLayer = new ShapeFileFeatureLayer(shapeFileName);
            featureLayer.ZoomLevelSet.ZoomLevel01.ApplyUntilZoomLevel = ApplyUntilZoomLevel.Level20;
            featureLayer.ZoomLevelSet.ZoomLevel01.CustomStyles.Add(GetPointStyle());
            //featureLayer.FeatureSource.Projection = GetProjection();

            var layerOverlay = new LayerOverlay();
            layerOverlay.Layers.Add(featureLayer);
            using (Bitmap bitmap = new Bitmap(256, 256))
            {
                var geoCanvas = new GdiPlusGeoCanvas();
                RectangleShape boundingBox = WebApiExtentHelper.GetBoundingBoxForXyz(x, y, z, GeographyUnit.Meter);
                geoCanvas.BeginDrawing(bitmap, boundingBox, GeographyUnit.Meter);
                layerOverlay.Draw(geoCanvas);
                geoCanvas.EndDrawing();

                MemoryStream memoryStream = new MemoryStream();
                bitmap.Save(memoryStream, ImageFormat.Png);

                HttpResponseMessage responseMessage = new HttpResponseMessage(HttpStatusCode.OK);
                responseMessage.Content = new ByteArrayContent(memoryStream.ToArray());
                responseMessage.Content.Headers.ContentType = new MediaTypeHeaderValue("image/png");

                return responseMessage;
            }
        }

        private static ManagedProj4Projection GetProjection()
        {
            var managedProj4Projection = new ManagedProj4Projection();
            managedProj4Projection.InternalProjectionParametersString = Proj4Projection.GetWgs84ParametersString();
            managedProj4Projection.ExternalProjectionParametersString = Proj4Projection.GetSphericalMercatorParametersString();

            return managedProj4Projection;
        }

        private static string GetFullPath(string relativePath)
        {
            Uri uri = new Uri(System.Reflection.Assembly.GetExecutingAssembly().CodeBase);
            string folderPath = Path.GetDirectoryName(Path.GetDirectoryName(uri.LocalPath));
            return Path.Combine(folderPath, relativePath);
        }

        private static PointStyle GetPointStyle()
        {
            /*var imagePath = GetFullPath(@"Images\icon.png");
            return new PointStyle(new GeoImage(imagePath));*/

            return new PointStyle(PointSymbolType.Circle, new GeoSolidBrush(GeoColors.BrightBlue), 5);
        }
    }
}