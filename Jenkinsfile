pipeline {
    agent { label 'agent-linux' }

    environment { 
        KUBERNETES_SERVER                   = credentials('KUBERNETES_SERVER')
        KUBERNETES_CLUSTER_CERTIFICATE      = credentials('KUBERNETES_CLUSTER_CERTIFICATE')
        KUBERNETES_TOKEN                    = credentials('KUBERNETES_TOKEN')
        DOCKER_CREDS                        = credentials('docker-credentials')
        IMAGE_NAME                          = 'dispro/dispro.accounts'
    }

    stages {
        stage('Build Docker Image') {
            steps {
                echo 'Building docker image...'
                sh "docker build -t $IMAGE_NAME -f ./DisPro.Accounts/Dockerfile ."
            }
        }
        stage ('Push Docker Image') {
            steps {
                echo 'Pushing docker image...'
                sh "./Scripts/docker-login.sh"
                sh "docker tag $IMAGE_NAME:latest $IMAGE_NAME:$BUILD_NUMBER"
                sh "docker push $IMAGE_NAME:latest"
                sh "docker push $IMAGE_NAME:$BUILD_NUMBER"
            }
        }
        stage('Test') {
            steps {
                echo 'Testing..'
            }
        }
        stage('Migrate Databases'){
            steps {
                echo 'Migrating databases...'
                sh "docker run $IMAGE_NAME:$BUILD_NUMBER --seed"
            }
        }
        stage('Deploy') {
            steps {
                echo 'Deploying....'
                sh "./Scripts/jenkins-deploy.sh"
            }
        }
    }
}