using GMImagePicker;
using Photos;
using UIKit;

namespace MigrationFormsExampleToMAUI.DependencyServices;

	/// <summary>
	/// iOS Multiple Image picker class implementation of Dependency Service IMultiImagePicker interface
	/// </summary>
	public partial class MultiImagePicker
	{
		/// <summary>
		/// Picks multiple photos from the Photos Application and invokes the completion handler after selection
		/// </summary>
		/// <param name="completionHandler"><completion handler callback on image selection/param>
		public partial void PickPhotosAsync(Action<List<ImageSource>> completionHandler)
		{
			var picker = new GMImagePickerController();
			picker.AllowsMultipleSelection = true;
			picker.DisplayAlbumsNumberOfAssets = true;
			picker.MediaTypes = new[] { PHAssetMediaType.Image };
			picker.GridSortOrder = SortOrder.Descending;
			picker.ModalPresentationStyle = UIKit.UIModalPresentationStyle.FullScreen;

			// Cancelled
			picker.Canceled += (sender, e) =>
			{
				// cancellation handler
			};

			// Picked images
			picker.FinishedPickingAssets += async (sender, args) =>
			{
				var list = new List<ImageSource>();
				ImageSource imageSource = ImageSource.FromFile(string.Empty); // empty source
				string filePath = string.Empty;
				foreach (PHAsset asset in args?.Assets)
				{
					filePath = await GetAbsoluteUrl(asset);
					if (!string.IsNullOrEmpty(filePath))
					{
						imageSource = ImageSource.FromFile(filePath);
					}
					list.Add(imageSource);
				}
				completionHandler?.Invoke(list);
			};

			var viewController = (UIApplication.SharedApplication.Delegate as AppDelegate).Window?.RootViewController;
			if (viewController != null)
			{
				viewController.PresentViewController(picker, true, null);
			}
		}

		/// <summary>
		/// Get the Absolute Url from PHAsset
		/// </summary>
		/// <param name="asset"></param>
		/// <returns></returns>
		private Task<string> GetAbsoluteUrl(PHAsset asset)
		{
			var taskSource = new TaskCompletionSource<string>();
			var optionEditing = new PHContentEditingInputRequestOptions();
			PHContentEditingHandler handler = (contentEditingInput, info) =>
			{
				taskSource.TrySetResult(contentEditingInput?.FullSizeImageUrl?.Path);
			};

			asset.RequestContentEditingInput(optionEditing, completionHandler: handler);
			return taskSource.Task;
		}
	}