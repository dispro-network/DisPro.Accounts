pipeline {
    agent { label 'agent-linux' }

    environment { 
        KUBERNETES_SERVER                   = credentials('KUBERNETES_SERVER')
        KUBERNETES_CLUSTER_CERTIFICATE      = credentials('KUBERNETES_CLUSTER_CERTIFICATE')
        KUBERNETES_TOKEN                    = credentials('KUBERNETES_TOKEN')
        DOCKER_CREDS                        = credentials('docker-credentials')
        IMAGE_NAME                          = 'dispro/dispro.accounts'
        IMAGE_TAG                           = "$GIT_BRANCH-$BUILD_NUMBER"
        SECRETS_VOL                         = "$GIT_BRANCH-secrets"
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
                sh "docker tag $IMAGE_NAME:latest $IMAGE_NAME:$IMAGE_TAG"
                sh "docker push $IMAGE_NAME:latest"
                sh "docker push $IMAGE_NAME:$IMAGE_TAG"
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
                withCredentials([file(credentialsId: 'DisPro.Accounts_Migrator', variable: 'FILE')]) {
                    dir('secrets') {
                        script {
                            try {
                                sh "docker volume create $SECRETS_VOL"
                                sh "cat $FILE |  docker run -i --rm -v=$SECRETS_VOL:/tmp/secrets ubuntu /bin/bash -c 'cat > /tmp/secrets/appsettings.secrets.json'"
                                // First destroy database (DevelopmentServer env only)
                                sh "docker run --rm -e ASPNETCORE_ENVIRONMENT=DevelopmentServer --mount type=volume,source=$SECRETS_VOL,target=/app/secrets $IMAGE_NAME:$IMAGE_TAG destroy"
                                sh "docker run --rm -e ASPNETCORE_ENVIRONMENT=DevelopmentServer --mount type=volume,source=$SECRETS_VOL,target=/app/secrets $IMAGE_NAME:$IMAGE_TAG migrate"
                            }
                            catch (ex) {
                                error('Aborting')
                            }
                            finally {
                                sh "docker volume rm -f $SECRETS_VOL"
                            }
                        }
                    }
                }
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