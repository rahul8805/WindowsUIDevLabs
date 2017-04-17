//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THE SOFTWARE IS PROVIDED “AS IS”, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, 
// INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, 
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. 
// IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
// DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, 
// TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH 
// THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//
//*********************************************************


using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Composition;
using Windows.UI.Core;

using Microsoft.UI.Composition.Toolkit;
using Windows.UI.Xaml;

namespace CompositionImageSample
{
    class CompositionImageSample : IFrameworkView
    {
        void IFrameworkView.Initialize(CoreApplicationView view)
        {
            _view = view;
        }

        void IFrameworkView.SetWindow(CoreWindow window)
        {
            _window = window;
            InitNewComposition();
        }

        void IFrameworkView.Load(string unused)
        {

        }

        void IFrameworkView.Run()
        {
            _window.Activate();
            _window.Dispatcher.ProcessEvents(CoreProcessEventsOption.ProcessUntilQuit);
        }

        void IFrameworkView.Uninitialize()
        {
            _window = null;
            _view = null;
        }

        void InitNewComposition()
        {
            _compositor = new Compositor();

            _root = _compositor.CreateContainerVisual();

            _compositionTarget = _compositor.CreateTargetForCurrentView();
            _compositionTarget.Root = _root;

            CreateChildElement();
        }

        void CreateChildElement()
        {
            Uri localUri = new Uri("ms-appx:///Assets/Kitten.png");
            _imageFactory = CompositionImageFactory.CreateCompositionImageFactory(_compositor);
            CompositionImageOptions options = new CompositionImageOptions()
            {
                DecodeWidth = 400,
                DecodeHeight = 400,
            };

            _image = _imageFactory.CreateImageFromUri(localUri, options);
            var v1_0 = _compositor.CreateSpriteVisual();
            v1_0.Size = new Vector2(200.0f, 200.0f);
            var surfaceBrush = _compositor.CreateSurfaceBrush(_image.Surface);
            surfaceBrush.HorizontalAlignmentRatio = 0.9f;
            surfaceBrush.VerticalAlignmentRatio = 0.0f;
            surfaceBrush.Stretch = CompositionStretch.Fill;

            v1_0.Brush = surfaceBrush;
            v1_0.CenterPoint = new Vector3(v1_0.Size / 2, 0);

            var v1_1 = _compositor.CreateSpriteVisual();
            v1_1.Size = new Vector2(400.0f, 400.0f);
            v1_1.Brush = _compositor.CreateColorBrush(Colors.Blue);
            //v1_1.Opacity = 0.5f;

            var v2_0 = _compositor.CreateSpriteVisual();
            v2_0.Size = new Vector2(100, 500.0f);
            v2_0.Brush = _compositor.CreateColorBrush(Colors.Red);
            //v2_0.Opacity = 0.5f;

            var v2_1 = _compositor.CreateSpriteVisual();
            v2_1.Size = new Vector2(200, 450.0f);
            v2_1.Brush = _compositor.CreateColorBrush(Colors.Green);

            _root.Children.InsertAtTop(v1_1); //b
            _root.Children.InsertAtTop(v2_0); //r
            _root.Children.InsertAtTop(v1_0); //k
            v1_1.Children.InsertAtTop(v2_1); //g

            ScalarKeyFrameAnimation animation = _compositor.CreateScalarKeyFrameAnimation();
            v1_0.Opacity = 1.0f;
            animation.InsertKeyFrame(1.0f, 360.0f);
            animation.Duration = TimeSpan.FromSeconds(10);
            animation.IterationBehavior = AnimationIterationBehavior.Forever;
            v1_0.StartAnimation("RotationAngleInDegrees", animation);

            ExpressionAnimation parallaxAnimation = _compositor.CreateExpressionAnimation("numerator*kittenVisual.RotationAngleInDegrees");

            float factor = 1.0f / 360f;
            parallaxAnimation.SetScalarParameter("numerator", factor);
            parallaxAnimation.SetReferenceParameter("kittenVisual", v1_0);

            // Start the animation on the background object
            //
            v1_1.StartAnimation("Opacity", parallaxAnimation);

            // If for some reason the image fails to load, replace the brush with
            // a red solid color.
            _image.ImageLoaded += (CompositionImage sender, CompositionImageLoadStatus status) =>
            {
                if (status != CompositionImageLoadStatus.Success)
                {
                    v1_0.Brush = _compositor.CreateColorBrush(Colors.Red);
                }
            };
        }

        // CoreWindow / CoreApplicationView
        private CoreWindow _window;
        private CoreApplicationView _view;

        // Windows.UI.Composition
        private Compositor _compositor;
        private CompositionTarget _compositionTarget;
        private ContainerVisual _root;

        // Microsoft.UI.Composition.Toolkit
        private CompositionImageFactory _imageFactory;
        private CompositionImage _image;
    }


    public sealed class CompositionImageSampleFactory : IFrameworkViewSource
    {
        IFrameworkView IFrameworkViewSource.CreateView()
        {
            return new CompositionImageSample();
        }

        static int Main(string[] args)
        {
            CoreApplication.Run(new CompositionImageSampleFactory());

            return 0;
        }
    }
}
