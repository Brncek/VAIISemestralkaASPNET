Installation Guide for Docker Desktop

Step 1: Download the Docker image from the following link: https://drive.google.com/file/d/1fNV3ZBv8TRy7CpewfjkAxwjQcgjszAMD/view?usp=sharing

Step 2: Navigate to the location of the downloaded file using your terminal.

Step 3: Load the Docker image
Run the following command in your terminal:
docker load -i dockerImage.tar

Once completed, the Docker Desktop application should display the image.

![image](https://github.com/user-attachments/assets/56dc0c8d-bbd9-4122-b93f-0fb00c735ee1)

Step 4:
Press the Run button for the image in Docker Desktop.
A new window titled Run a New Container will open.
Expand the Optional Settings section.
Set a name for your container and specify the port for the HTTP server.
Note: Port 80 is commonly used for HTTP servers, but you can specify any port you prefer.

![image](https://github.com/user-attachments/assets/dd27d7ac-a8fa-45d7-b991-7b9377a8b28b)

Step 5: 
Press the Run button.
Docker will prepare and start your container.
Once the server starts correctly, it should look something like this:

![image](https://github.com/user-attachments/assets/17b54054-d1b7-452d-b606-6f7cf05ce264)



Alternative: Running Without Docker Desktop

If you prefer not to use Docker Desktop, you can follow these steps:

Download the entire repository containing the project files.

Open the project in Visual Studio.

Select your preferred method of publishing the project. For example:

Publish the project to a folder on your system. Once the publishing process is complete, navigate to the folder and run the executable (.exe) file to start the application.
If you want to specifi the port navigate to appsettings.json open it and change the original port 5041 to a different one
