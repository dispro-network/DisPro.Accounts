pipeline {
    agent { label 'agent-linux' }

    stages {
        stage('Build') {
            steps {
                echo 'Building..'
                sh "docker build -t dispro/dispro-accounts -f ./DisPro.Accounts/Dockerfile ."
            }
        }
        stage('Test') {
            steps {
                echo 'Testing..'
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'
            }
        }
    }
}