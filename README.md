# Image Scanner App
This application lets users upload images, which are stored in Azure Blob Storage. Once uploaded, the images are analyzed using Microsoft Cognitive Services' Computer Vision AI API, which generates a detailed text description of the image's content. The description is then displayed to the user. To improve efficiency, the app caches responses, so if the same image is uploaded again, the cached description is used instead of reprocessing the image.

## How It Works
1. Upload an Image: Users upload an image through the web interface.
2. Image Stored in Azure: The image is saved in Azure Blob Storage.
3. AI Analyzes Image: The Microsoft Cognitive Services' Computer Vision API analyzes the image and returns a textual description.
4. Results Displayed: The AI-generated description is displayed to the user.
5. Caching: If the same image is uploaded again, the cached response is used.

## Ideal Audience
This application is ideal for developers interested in integrating Azure Blob Storage and Microsoft Cognitive Services for image processing, businesses needing automated image analysis for content management or tagging, educators and students exploring AI-based image recognition, content creators looking to generate descriptions or alt text for images to improve accessibility, and researchers working in AI, computer vision, or machine learning who need a quick and efficient image analysis tool.

## Technologies used
* C#
* .NET 8
* HTML
* CSS
* JavaScript
* Asp.NET Core MVC
* Azure Blob Storage
* Azure Computer Vision AI 
