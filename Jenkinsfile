pipeline {
  agent any
  stages {
    stage('Run app') {
      parallel {
        stage('Run app') {
          steps {
            sh 'dotnet run'
          }
        }

        stage('Run another command') {
          steps {
            echo 'Hello'
          }
        }

      }
    }

  }
}