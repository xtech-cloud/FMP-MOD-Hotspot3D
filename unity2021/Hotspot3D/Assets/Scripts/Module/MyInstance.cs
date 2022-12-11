using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using LibMVCS = XTC.FMP.LIB.MVCS;
using XTC.FMP.MOD.Hotspot3D.LIB.Proto;
using XTC.FMP.MOD.Hotspot3D.LIB.MVCS;
using UnityEngine.EventSystems;
using System.Linq;
using System.IO;
using Newtonsoft.Json;
using System;

namespace XTC.FMP.MOD.Hotspot3D.LIB.Unity
{
    /// <summary>
    /// 实例类
    /// </summary>
    public class MyInstance : MyInstanceBase
    {
        public class UiReference
        {
            public RawImage renderer;
        }

        public class WorldReference
        {
            public Camera camera;
            public Transform slot;
            public Transform hotspot;
        }

        private UiReference uiReference_ = new UiReference();
        private WorldReference worldReference_ = new WorldReference();
        private ContentMetaSchema activeContent_;
        private ContentReader contentReader_;

        public MyInstance(string _uid, string _style, MyConfig _config, MyCatalog _catalog, LibMVCS.Logger _logger, Dictionary<string, LibMVCS.Any> _settings, MyEntryBase _entry, MonoBehaviour _mono, GameObject _rootAttachments)
            : base(_uid, _style, _config, _catalog, _logger, _settings, _entry, _mono, _rootAttachments)
        {
        }

        /// <summary>
        /// 当被创建时
        /// </summary>
        /// <remarks>
        /// 可用于加载主题目录的数据
        /// </remarks>
        public void HandleCreated()
        {
            contentReader_ = new ContentReader(contentObjectsPool);
            contentReader_.AssetRootPath = settings_["path.assets"].AsString();
            contentReader_.ContentUri = "";

            rootUI.transform.Find("bg").gameObject.SetActive(false);

            uiReference_.renderer = rootUI.transform.Find("renderer").GetComponent<RawImage>();
            worldReference_.camera = rootWorld.transform.Find("camera").GetComponent<Camera>();
            worldReference_.slot = rootWorld.transform.Find("[slot]");
            worldReference_.hotspot = rootWorld.transform.Find("[slot]/hotspot");
            worldReference_.hotspot.gameObject.AddComponent<HotspotClickHandler>();
            worldReference_.hotspot.gameObject.SetActive(false);
            worldReference_.hotspot.GetComponent<MeshRenderer>().enabled = style_.hotspot.debugBox.visible;

            int renderTextureWidth = (int)uiReference_.renderer.rectTransform.rect.width;
            int renderTextureHeight = (int)uiReference_.renderer.rectTransform.rect.height;
            var renderTexture = new RenderTexture(renderTextureWidth, renderTextureHeight, 16, RenderTextureFormat.ARGB32);
            renderTexture.Create();
            uiReference_.renderer.texture = renderTexture;
            worldReference_.camera.targetTexture = renderTexture;

            var clickHandler = uiReference_.renderer.gameObject.AddComponent<RendererPointerClickHandler>();
            clickHandler.OnClick = onRendererClick;

            Func<string, Color> convertColor = (_color) =>
            {
                Color color = Color.white;
                if (!ColorUtility.TryParseHtmlString(_color, out color))
                    color = Color.white;
                return color;
            };
            worldReference_.hotspot.GetComponent<MeshRenderer>().material.color = convertColor(style_.hotspot.debugBox.color);

            //添加手势事件
            wrapGesture();
        }

        /// <summary>
        /// 当被删除时
        /// </summary>
        public void HandleDeleted()
        {
        }

        /// <summary>
        /// 当被打开时
        /// </summary>
        /// <remarks>
        /// 可用于加载内容目录的数据
        /// </remarks>
        public void HandleOpened(string _source, string _uri)
        {
            rootWorld.transform.localPosition = new Vector3(
                style_.spaceGrid.position.x,
                style_.spaceGrid.position.y,
                style_.spaceGrid.position.z
                );

            worldReference_.camera.transform.localPosition = new Vector3(
                style_.renderCamera.position.x,
                style_.renderCamera.position.y,
                style_.renderCamera.position.z
                );

            worldReference_.camera.transform.localEulerAngles = new Vector3(
                style_.renderCamera.rotation.x,
                style_.renderCamera.rotation.y,
                style_.renderCamera.rotation.z
                );

            rootUI.gameObject.SetActive(true);
            rootWorld.gameObject.SetActive(true);
            applyCatalog();
        }

        /// <summary>
        /// 当被关闭时
        /// </summary>
        public void HandleClosed()
        {
            rootUI.gameObject.SetActive(false);
            rootWorld.gameObject.SetActive(false);
        }

        public void applyCatalog()
        {
            foreach (var section in catalog_.sectionS)
            {
                var clone = GameObject.Instantiate(worldReference_.hotspot.gameObject, worldReference_.hotspot.transform.parent);
                clone.SetActive(true);

                string strPositionX = "0";
                section.kvS.TryGetValue("positionX", out strPositionX);
                float positionX = 0;
                float.TryParse(strPositionX, out positionX);
                string strPositionY = "0";
                section.kvS.TryGetValue("positionY", out strPositionY);
                float positionY = 0;
                float.TryParse(strPositionY, out positionY);
                string strPositionZ = "0";
                section.kvS.TryGetValue("positionZ", out strPositionZ);
                float positionZ = 0;
                float.TryParse(strPositionZ, out positionZ);

                clone.transform.localPosition = new Vector3(positionX, positionY, positionZ);

                string strScaleX = "1";
                section.kvS.TryGetValue("scaleX", out strScaleX);
                float scaleX = 0;
                float.TryParse(strScaleX, out scaleX);
                string strScaleY = "1";
                section.kvS.TryGetValue("scaleY", out strScaleY);
                float scaleY = 0;
                float.TryParse(strScaleY, out scaleY);
                string strScaleZ = "1";
                section.kvS.TryGetValue("scaleZ", out strScaleZ);
                float scaleZ = 0;
                float.TryParse(strScaleZ, out scaleZ);

                clone.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

                clone.GetComponent<HotspotClickHandler>().OnClick = () =>
                {
                    activeContent_ = null;
                    // 读取 content
                    var firstSection = section.contentS.First();
                    string metafile = Path.Combine(firstSection, "meta.json");
                    contentReader_.LoadText(metafile, (_text) =>
                    {
                        try
                        {
                            activeContent_ = JsonConvert.DeserializeObject<ContentMetaSchema>(System.Text.Encoding.UTF8.GetString(_text));
                        }
                        catch (System.Exception ex)
                        {
                            logger_.Exception(ex);
                            return;
                        }

                        logger_.Debug("hotspot on");
                        openResource();
                    }, () => { });
                };
            }
        }

        /// <summary>
        /// 封装手势操作
        /// </summary>
        private void wrapGesture()
        {
            //var camera = worldReference_.camera;
            // 接收手势事件的对象
            var gestureOwner = uiReference_.renderer.gameObject;
            // 手势操作的对象
            var gestureTarget = worldReference_.slot;
            // 水平滑动
            var swipeH = gestureOwner.AddComponent<HedgehogTeam.EasyTouch.QuickSwipe>();
            swipeH.swipeDirection = HedgehogTeam.EasyTouch.QuickSwipe.SwipeDirection.Horizontal;
            swipeH.onSwipeAction = new HedgehogTeam.EasyTouch.QuickSwipe.OnSwipeAction();
            swipeH.onSwipeAction.AddListener((_gesture) =>
            {
                if (uiReference_.renderer.gameObject != _gesture.pickedUIElement)
                    return;
                // 忽略摄像机视窗外
                /*
                if (_gesture.position.x < camera.pixelRect.x ||
                _gesture.position.x > camera.pixelRect.x + camera.pixelRect.width ||
                _gesture.position.y < camera.pixelRect.y ||
                _gesture.position.y > camera.pixelRect.y + camera.pixelRect.height)
                    return;
                */
                var vec = gestureTarget.localRotation.eulerAngles;
                if (style_.yawAxis.invert)
                    vec.y = vec.y + _gesture.swipeVector.x;
                else
                    vec.y = vec.y - _gesture.swipeVector.x;
                // 将[0.360]转换到[-180,180]区间
                if (vec.y > 180)
                    vec.y = vec.y - 360;
                // 限制范围
                if (vec.y > style_.yawAxis.rangeMax)
                    vec.y = style_.yawAxis.rangeMax;
                if (vec.y < style_.yawAxis.rangeMin)
                    vec.y = style_.yawAxis.rangeMin;
                gestureTarget.localRotation = Quaternion.Euler(vec.x, vec.y, vec.z);
            });
            // 垂直滑动
            var swipeV = gestureOwner.AddComponent<HedgehogTeam.EasyTouch.QuickSwipe>();
            swipeV.swipeDirection = HedgehogTeam.EasyTouch.QuickSwipe.SwipeDirection.Vertical;
            swipeV.onSwipeAction = new HedgehogTeam.EasyTouch.QuickSwipe.OnSwipeAction();
            swipeV.onSwipeAction.AddListener((_gesture) =>
            {
                if (uiReference_.renderer.gameObject != _gesture.pickedUIElement)
                    return;
                // 忽略摄像机视窗外
                /*
                if (_gesture.position.x < camera.pixelRect.x ||
                _gesture.position.x > camera.pixelRect.x + camera.pixelRect.width ||
                _gesture.position.y < camera.pixelRect.y ||
                _gesture.position.y > camera.pixelRect.y + camera.pixelRect.height)
                    return;
                */
                var vec = gestureTarget.localRotation.eulerAngles;
                if (style_.pitchAxis.invert)
                    vec.x = vec.x - _gesture.swipeVector.y;
                else
                    vec.x = vec.x + _gesture.swipeVector.y;
                // 将[0.360]转换到[-180,180]区间
                if (vec.x > 180)
                    vec.x = vec.x - 360;
                // 限制范围
                if (vec.x > style_.pitchAxis.rangeMax)
                    vec.x = style_.pitchAxis.rangeMax;
                if (vec.x < style_.pitchAxis.rangeMin)
                    vec.x = style_.pitchAxis.rangeMin;
                gestureTarget.rotation = Quaternion.Euler(vec.x, vec.y, vec.z);
            });
            // 捏合
            /*
            var pinch = _camera.gameObject.AddComponent<HedgehogTeam.EasyTouch.QuickPinch>();
            pinch.onPinchAction.AddListener((_gesture) =>
            {
                // 忽略摄像机视窗外
                if (_gesture.position.x < camera.pixelRect.x ||
                _gesture.position.x > camera.pixelRect.x + camera.pixelRect.width ||
                _gesture.position.y < camera.pixelRect.y ||
                _gesture.position.y > camera.pixelRect.y + camera.pixelRect.height)
                    return;
                _camera.GetComponent<Camera>().fieldOfView *= _gesture.deltaPinch;
            });
            */
        }

        private void onRendererClick(PointerEventData _eventData)
        {
            //_eventData.position: 屏幕左下角（0，0） 右上角（屏幕宽，屏幕高）
            //clickPositionRenderer: 屏幕左下角（0，0） 右上角（canvas宽，canvas高)

            // 转换后的在renderer中的点击位置
            Vector2 clickPositionInRenderer;

            /*
            将一个屏幕空间点转换为 RectTransform 的本地空间中位于其矩形平面上的一个位置。
            cam 参数应为与此屏幕点关联的摄像机。对于设置为 Screen Space - Overlay 模式的 Canvas 中的 RectTransform，cam 参数应为 null。
            当从提供 PointerEventData 对象的事件处理程序中使用 ScreenPointToLocalPointInRectangle 时，可以通过使用 PointerEventData.enterEventData（对于悬停功能）或 PointerEventData.pressEventCamera（对于单击功能）获取正确的摄像机。这会为给定事件自动使用正确的摄像机（或 null）。
            */
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                uiReference_.renderer.rectTransform, _eventData.position,
                _eventData.pressEventCamera, out clickPositionInRenderer))
            {
                //Debug.Log(_eventData.position);
                //Debug.Log(clickPositionInRenderer);

                float rendererWidth = uiReference_.renderer.rectTransform.rect.width;
                float rendererHeight = uiReference_.renderer.rectTransform.rect.height;

                float x = clickPositionInRenderer.x / rendererWidth;
                float y = clickPositionInRenderer.y / rendererHeight;

                //视口坐标是标准化的、相对于摄像机的坐标。摄像机左下角为 (0,0)，右上角为 (1,1)。
                Ray ray = worldReference_.camera.ViewportPointToRay(new Vector2(x, y));
                RaycastHit raycastHit;
                if (Physics.Raycast(ray, out raycastHit))
                {
                    var clickHandler = raycastHit.transform.GetComponent<HotspotClickHandler>();
                    if (null == clickHandler)
                        return;
                    clickHandler.OnClick();
                }
            }

        }

        /// <summary>
        /// 打开资源
        /// </summary>
        private void openResource()
        {
            if (null == activeContent_)
            {
                logger_.Error("None Content is active");
                return;
            }

            if (string.IsNullOrWhiteSpace(activeContent_.foreign_bundle_uuid))
            {
                logger_.Error("bundle is null or whitespace");
                return;
            }

            string strValue = "";
            activeContent_.kvS.TryGetValue(style_.hotspot.key, out strValue);
            if (string.IsNullOrWhiteSpace(strValue))
            {
                logger_.Error("content.kv.{0} is null or whitespace", MyEntryBase.ModuleName);
                return;
            }

            Dictionary<string, object> variableS = new Dictionary<string, object>();
            variableS["{{uri}}"] = string.Format("{0}/{1}", activeContent_.foreign_bundle_uuid, strValue);
            publishSubjects(style_.hotspot.onSubjectS, variableS);
        }
    }
}
