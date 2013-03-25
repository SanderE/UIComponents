using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Android.Widget;
using Android.Views;
using Android.Graphics;
using Android.Content;

namespace Macaw.UIComponents
{
    public class MultiImageView : ImageView, GestureDetector.IOnGestureListener
    {
        protected static Dictionary<string, Bitmap> IMAGE_CACHE = new Dictionary<string, Bitmap>();
        protected static int CURRENT_IMAGE = 0;

        private List<string> imageList;
        private int imageCount;
        private GestureDetector gestureDetector;

        private int magnifyHeight;
        /// <summary>
        /// Returns the Height of the Magnify icon, to set this use SetMagnifyIconDimensions(height, width)
        /// </summary>
        public int MagnifyHeight { get { return magnifyHeight; } }
        private int magnifyWidth;

        /// <summary>
        /// Returns the Width of the Magnify icon, to set this use SetMagnifyIconDimensions(height, width)
        /// </summary>
        public int MagnifyWidth { get { return magnifyWidth; } }

        private int sliderIconHeight;
        /// <summary>
        /// Returns the Height of the slider icons, to set this use SetSliderIconDimensions(height, width)
        /// </summary>
        public int SliderIconHeight { get { return sliderIconHeight; } }
        private int sliderIconWidth;
        /// <summary>
        /// Returns the Width of the slider icons, to set this use SetSliderIconDimensions(height, width)
        /// </summary>
        public int SliderIconWidth { get { return sliderIconWidth; } }

        /// <summary>
        /// Decides the SampleSize at which images are downloaded when using LoadImageList(string[] urls). Default value is 1
        /// </summary>
        public int DownloadedImageSampleSize { get; set; }
        /// <summary>
        /// Sets wether or not the Magnify event in the top left is enabled. When Enabled will draw a MagnifyIcon (if provided through its property) in the top left and fires a "ZoomImageEvent" when clicked within its boundaries. The boundaries can be set through SetMagnifyIconDimensions(height, width)
        /// </summary>
        public bool MagnifyEnabled { get; set; }
        private Bitmap magnifyIcon;
        /// <summary>
        /// Sets the icon used in the top left for the ZoomImageEvent. Only shows of MagnifyEnabled is true. Dimensions can be set through SetMagnifyIconDimensions(height, width)
        /// </summary>
        public Bitmap MagnifyIcon { get { return magnifyIcon; } set { magnifyIcon = Bitmap.CreateScaledBitmap(value, MagnifyWidth, MagnifyHeight, true); } }

        private Bitmap sliderSelectedIcon;
        /// <summary>
        /// Sets the icon used for the active image. Slider icons only show if both SliderSelectedIcon and SliderUnselectedIcon are set.
        /// </summary>
        public Bitmap SliderSelectedIcon { get { return sliderSelectedIcon; } set { sliderSelectedIcon = Bitmap.CreateScaledBitmap(value, sliderIconWidth, sliderIconHeight, true); } }

        private Bitmap sliderUnselectedIcon;
        /// <summary>
        /// Sets the icon used for the non-active images. Slider icons only show if both SliderSelectedIcon and SliderUnselectedIcon are set.
        /// </summary>
        public Bitmap SliderUnselectedIcon { get { return sliderUnselectedIcon; } set { sliderUnselectedIcon = Bitmap.CreateScaledBitmap(value, sliderIconWidth, sliderIconHeight, true); } }

        #region Constructors

        public MultiImageView(Context context)
            : base(context)
        {
            SharedConstructor();
        }

        public MultiImageView(Context context, global::Android.Util.IAttributeSet attrs)
            : base(context, attrs)
        {
            SharedConstructor();
        }

        private void SharedConstructor()
        {
            imageList = new List<string>();
            gestureDetector = new GestureDetector(this);
            imageCount = 0;

            DownloadedImageSampleSize = 1;
            MagnifyEnabled = true;
            magnifyHeight = 70;
            magnifyWidth = 70;

            sliderIconHeight = 30;
            sliderIconWidth = 30;
        }

        #endregion

        protected override void OnDraw(Canvas canvas)
        {
            if ((imageList.Count != 0))
            {
                if (CURRENT_IMAGE > imageList.Count - 1)
                {
                    CURRENT_IMAGE = 0;
                }

                try
                {
                    SetImageBitmap(IMAGE_CACHE[imageList.ElementAt<string>(CURRENT_IMAGE)]);
                }
                catch(Exception)
                {
                }
            }

            base.OnDraw(canvas);

            Paint p = new Paint(PaintFlags.AntiAlias);
            p.Color = Color.Black;
            p.SetStyle(Paint.Style.Stroke);
            p.StrokeWidth = 1f;
            if (MagnifyEnabled && MagnifyIcon != null)
            {
                canvas.DrawBitmap(MagnifyIcon, 0f, 0f, p);
            }


            if(SliderSelectedIcon != null && SliderUnselectedIcon != null)
            {
                if (imageList.Count > 1)
                {
                    p.Color = Color.Red;
                    int offset = 0;
                    for (int i = 0; i < imageList.Count; i++)
                    {
                        int width = Convert.ToInt16(Math.Floor((this.MeasuredWidth * 50f) / 100f)) - ((imageList.Count - 1) * SliderUnselectedIcon.Width / 2) - (SliderUnselectedIcon.Width / 2);
                        int height = Convert.ToInt16(Math.Floor((this.MeasuredHeight * 80f) / 100f));
                        if (i == CURRENT_IMAGE) { canvas.DrawBitmap(SliderSelectedIcon, width + offset, height, p); } else { canvas.DrawBitmap(SliderUnselectedIcon, width + offset, height, p); }
                        offset += SliderUnselectedIcon.Width + 5; //45;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the dimensions of the slider icons, both active and non active
        /// </summary>
        /// <param name="height">Determines the icons' height in pixels.</param>
        /// <param name="width">Determines the icons' width in pixels</param>
        public void SetSliderIconDimensions(int height, int width)
        {
            sliderIconWidth = width;
            sliderIconHeight = height;
            if (MagnifyIcon != null)
            {
                SliderSelectedIcon = Bitmap.CreateScaledBitmap(SliderSelectedIcon, sliderIconWidth, sliderIconHeight, true);
                SliderUnselectedIcon = Bitmap.CreateScaledBitmap(SliderUnselectedIcon, sliderIconWidth, sliderIconHeight, true);
            }
        }

        /// <summary>
        /// Sets the dimensions of the magnify icon, both active and non active
        /// </summary>
        /// <param name="height">Determines the icons' height in pixels.</param>
        /// <param name="width">Determines the icons' width in pixels</param>
        public void SetMagnifyIconDimensions(int height, int width)
        {
            magnifyWidth = width;
            magnifyHeight = height;
            if (MagnifyIcon != null)
            {
                MagnifyIcon = Bitmap.CreateScaledBitmap(MagnifyIcon, MagnifyWidth, MagnifyHeight, true);
            }
        }

        private void ShowFirstImageInCache(int delay)
        {
            System.Threading.Thread.Sleep(delay);
            try
            {
                SetImageBitmap(IMAGE_CACHE.Values.First());
            }
            catch (Exception)
            {
            }
        }

        public bool OnDown(MotionEvent e)
        {
            return true;
        }

        public bool OnFling(MotionEvent e1, MotionEvent e2, float velocityX, float velocityY)
        {
            if ((e1.GetX() - e2.GetX()) < -100f)
            {
                LoadNextImage();
                return false;
            }
            else if ((e1.GetX() - e2.GetX()) > 100f)
            {
                LoadPreviousImage();
                return false;
            }
            return true;
        }

        public void OnLongPress(MotionEvent e)
        {
        }

        public bool OnScroll(MotionEvent e1, MotionEvent e2, float distanceX, float distanceY)
        {
            return false;
        }

        public void OnShowPress(MotionEvent e)
        {
        }

        public bool OnSingleTapUp(MotionEvent e)
        {
            if (MagnifyEnabled)
            {
                if ((e.GetX() < MagnifyWidth + 30f) && (e.GetY() < MagnifyHeight + 30f))
                {
                    // Pressed top left!
                    OnZoomImageEvent(EventArgs.Empty);
                }
            }
            return true;
        }

        /// <summary>
        /// Fires on tap of the Magnify Icon if the MagnifyEnabled is true and a MagnifyIcon is set.
        /// </summary>
        public event EventHandler ZoomImageEvent;

        protected void OnZoomImageEvent(EventArgs e)
        {
            if (ZoomImageEvent != null)
            {
                ZoomImageEvent(this, e);
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            return gestureDetector.OnTouchEvent(e);
        }

        /// <summary>
        /// Downloads and caches a list of images that are set through URLs and fires the "ImagesLoaded" event whenever an image has successfully been downloaded. Does not automatically display the images downloaded in the active View, that should be handled on reaction of the ImagesLoaded event. For downloaded the images at a scaled resolution look at setting the DownloadedImageSampleSize property.
        /// </summary>
        /// <param name="imageUrls">A list of URLs with images that will be downloaded.</param>
        public void LoadImageList(string[] imageUrls)
        {
            imageList.Clear();
            imageCount = 0;

            for (int i = 0; i < imageUrls.Length; i++)
            {
                if (!String.IsNullOrEmpty(imageUrls[i]))
                {
                    imageCount++;
                    if (!IMAGE_CACHE.ContainsKey(imageUrls[i]))
                    {
                        //Console.WriteLine("Downloading image : " + imageUrls[i]);
                        WebClient webClientImageDownloader = new WebClient();
                        webClientImageDownloader.OpenReadCompleted += new OpenReadCompletedEventHandler(webClientImageDownloader_OpenReadCompleted);
                        webClientImageDownloader.BaseAddress = imageUrls[i];
                        webClientImageDownloader.OpenReadAsync(new Uri(imageUrls[i], UriKind.Absolute));
                    }
                    else
                    {
                        imageList.Add(imageUrls[i]);
                    }

                    if (imageList.Count == 1)
                    {
                        // Raise event that image has been loaded
                        OnImageLoaded(EventArgs.Empty);
                    }
                }
            }
            RemoveOldImagesFromCache();
        }

        /// <summary>
        /// Loads and caches a list of given Bitmap objects. Fires the ImagesLoaded event whenever an image is loaded into the list.
        /// </summary>
        /// <param name="images">A list of Bitmaps to be saved and cycled through</param>
        public void LoadImageList(Bitmap[] images)
        {
            imageList.Clear();
            imageCount = 0;

            for (int i = 0; i < images.Length; i++)
            {
                if (images[i] != null)
                {
                    imageCount++;
                    if (!IMAGE_CACHE.ContainsKey(images[i].GetHashCode().ToString()))
                    {
                        IMAGE_CACHE.Add(images[i].GetHashCode().ToString(), images[i]);
                    }
                    else
                    {
                        imageList.Add(images[i].GetHashCode().ToString());
                    }

                    if (imageList.Count == 1)
                    {
                        // Raise event that image has been loaded
                        OnImageLoaded(EventArgs.Empty);
                    }
                }
            }
            RemoveOldImagesFromCache();
        }

        private void RemoveOldImagesFromCache()
        {
            try
            {
                HashSet<string> urls = new HashSet<string>(imageList);
                List<string> listToRemove = new List<string>();

                foreach (KeyValuePair<string, Bitmap> entry in IMAGE_CACHE)
                {
                    if (!urls.Contains(entry.Key))
                    {
                        listToRemove.Add(entry.Key);
                    }
                }

                foreach (string outdatedUrl in listToRemove)
                {
                    RemoveImageFromCache(outdatedUrl);
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// Clears the entire ImageView of all images as well as clearing the cache.
        /// </summary>
        public void ClearImageCache()
        {
            foreach (KeyValuePair<string, Bitmap> entry in IMAGE_CACHE)
            {
                entry.Value.Dispose();
            }
            IMAGE_CACHE.Clear();
            imageList.Clear();
        }

        private void RemoveImageFromCache(string url)
        {
            try
            {
                if (IMAGE_CACHE.ContainsKey(url))
                {
                    IMAGE_CACHE[url].Dispose();
                }
                IMAGE_CACHE.Remove(url);
            }
            catch
            {
            }
        }

        /// <summary>
        /// Event that fires whenever an image is successfully loaded into the cache.
        /// </summary>
        public event EventHandler ImagesLoaded;

        protected void OnImageLoaded(EventArgs e)
        {
            if (ImagesLoaded != null)
            {
                ImagesLoaded(this, e);
            }
        }

        private void webClientImageDownloader_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            try
            {
                var client = sender as WebClient;
                if (client != null)
                {
                    string url = client.BaseAddress;
                    var opts = new BitmapFactory.Options() { InPurgeable = true, InInputShareable = true };
                    opts.InSampleSize = DownloadedImageSampleSize;
                    Bitmap image = BitmapFactory.DecodeStream(e.Result, new Rect(-1, -1, -1, -1), opts);

                    IMAGE_CACHE.Add(url, image);
                    imageList.Add(url);
                    if (imageList.Count == 1)
                    {
                        // Raise event that image has been loaded
                        CURRENT_IMAGE = 0;
                    }
                    OnImageLoaded(EventArgs.Empty);
                }
                else
                {
                    // Failed to load image
                }
            }
            catch (Exception)
            {
                //throw ex;
            }
        }

        /// <summary>
        /// Loads the first image in the list.
        /// </summary>
        public void LoadImage()
        {
            try
            {
                SetImageBitmap(IMAGE_CACHE[imageList.ElementAt<string>(CURRENT_IMAGE)]);
            }
            catch (Exception)
            {
                CURRENT_IMAGE = 0;
                try
                {
                    SetImageBitmap(IMAGE_CACHE[imageList.First<string>()]);
                }
                catch (Exception)
                {
                    System.Threading.Thread.Sleep(500);
                    LoadImage();
                }
            }
            //this.Invalidate();
        }

        /// <summary>
        /// Loads the next image in the list. Not recommended to use, the MultiImageView automatically calls this on a right swipe.
        /// </summary>
        public void LoadNextImage()
        {
            if (imageList.Count != 0)
            {
                if (CURRENT_IMAGE + 1 > (imageList.Count - 1))
                {
                    CURRENT_IMAGE = -1;
                }

                try
                {
                    SetImageBitmap(IMAGE_CACHE[imageList.ElementAt<string>(++CURRENT_IMAGE)]);
                }
                catch (Exception)
                {
                }
            }
            this.Invalidate();
        }

        /// <summary>
        /// Loads the previous image in the list. Not recommended to use, the MultiImageView automatically calls this on a left swipe.
        /// </summary>
        public void LoadPreviousImage()
        {
            if (imageList.Count != 0)
            {
                if (CURRENT_IMAGE == 0)
                {
                    CURRENT_IMAGE = imageList.Count;
                }
                try
                {
                    SetImageBitmap(IMAGE_CACHE[imageList.ElementAt<string>(--CURRENT_IMAGE)]);
                }
                catch (Exception)
                {
                }
            }
            this.Invalidate();
        }
    }
}
