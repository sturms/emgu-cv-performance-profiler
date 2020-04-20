# What is this tool about?
A simple performance profiling tool that checks EmguCV image search speed via single or multiple threads. It searches a small image within larger one and draws a red rectangle in the coordinates where the image was found.
To go even further, there is used a custom image scaling functionality that scales down the images before performing the actual search. The less pixels to search, the faster the image search is!

# How does it work?
This tool is created using WPF, C# and EmguCV. It simply calls the EmguCV "MatchTemplate" function that returns a System.Drawing.Rectangle. Other part of application just handles the user inputs, scale images and run the search on either single or multiple threads.   

# Why was it created?
It was created just for curiosity, to be able to visualize the difference between execution speeds when changing different input parameters. Although, it may be quite handy if you want to measure the execution speed for specific image search scenarios including image scaling and multithreading.

# How to use it
When running the application .exe file, the following window will open: 
<p align="center"><img src="preview/main_window_initial_load.jpg" alt="Main window first opened" Width="60%" Height="auto" /></p>

User selects large image and small image, the directory structure can be something like this:
<p align="center"><img src="preview/folder_structure.jpg" alt="Folder structure" Width="60%" Height="auto" /></p>

As a sample for large image there is used a MS Word window screenshot.
<p align="center"><img src="preview/large_image.jpg" alt="Large image" Width="60%" Height="auto" /></p>

And the MS word functions will be small images (searched in the large image).
<p align="center"><img src="preview/small_images.jpg" alt="Small image" Width="60%" Height="auto" /></p>

Afer pressin the "Search" button, can see that the ellapsed time is almost 1sec for single image search.
<p align="center"><img src="preview/main_window_search_using_single_thread.jpg" alt="Main window search using single thread" 
                       Width="60%" Height="auto" /></p>

When opening the details view can see that the function have been found in the larger image (red rectangle in top left corner).
<p align="center"><img src="preview/view_result_details_image_loaded.jpg" alt="View result details for single thread search" /></p>

For single image search to complete in almost a second is quite slow. To improve that we can run this search in multiple threads so that the application facilitate all the PC cores. We can see that the execution speed creately improved!
<p align="center"><img src="preview/main_window_search_using_multithreading.jpg" alt="Main window search using multiple threads" Width="60%" Height="auto" /></p>

But this is not enough! Lets scale down the image and see that it completes in less than 100ms.
<p align="center"><img src="preview/main_window_search_using_multithrd_scaling.jpg" alt="Main window search using multiple threads (scaling made)" Width="60%" Height="auto" /></p>

The smaller image again was found. Can see that the preview is quite blurry as the large image was also scaled down.
<p align="center"><img src="preview/view_result_resized_image_selected_rect_shown.jpg" alt="View result details for multiple thread search" /></p>

The noticable difference in performance can be seen in following table:
<p align="center"><img src="preview/performance_profiling_results_excel_table.jpg" alt="Performance profiling results" Width="80%" Height="auto" /></p>









