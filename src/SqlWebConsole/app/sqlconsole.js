(function () {
    'use strict';

    angular
        .module('sql', ['ui.router', 'ui.bootstrap', 'ui.ace'])
        .constant('AUTH_EVENTS', {
            loginSuccess: 'auth-login-success',
            loginFailed: 'auth-login-failed',
            logoutSuccess: 'auth-logout-success',
            sessionTimeout: 'auth-session-timeout',
            notAuthenticated: 'auth-not-authenticated',
            notAuthorized: 'auth-not-authorized'
        })
        .run(onAuthorize)
        .run(onAuthenticateEvents)
        .config(configStates)
        .config(configHttpInterceptors)
        .directive('formAutofillFix', FormAutofillFixDirective)
        .factory('authService', AuthServiceFactory)
        .factory('authInterceptor', AuthInterceptorFactory)
        .service('session', SessionService)
        .controller('LoginController', LoginController)
        .controller('HomeController', HomeController)
        .controller('NewConnectonController', NewConnectonController);

    var lastState = null;
    function onAuthorize($rootScope, AUTH_EVENTS, authService) {


        $rootScope.$on('$stateChangeStart', function (event, next) {
            if (!authService.isAuthenticated()) {
                $rootScope.$broadcast(AUTH_EVENTS.notAuthenticated);
                event.preventDefault();
                lastState = next.name;
            }
        });

        $rootScope.setCurrentUser = function (user) {
            $rootScope.profile = user;
        };

        

    }

    function onAuthenticateEvents($rootScope, $state, $uibModal, AUTH_EVENTS) {

        var modalInstance = null;
        $rootScope.$on(AUTH_EVENTS.notAuthenticated, showDialog);
        $rootScope.$on(AUTH_EVENTS.sessionTimeout, showDialog);
        $rootScope.$on(AUTH_EVENTS.loginSuccess, hideDialog);

        function showDialog() {
            modalInstance = $uibModal.open({
                templateUrl: 'views/login-form.html',
                controller: 'LoginController as vm',
                backdrop: 'static',
                keyboard: false
            });
        }

        function hideDialog() {
            if (modalInstance != null) {
                modalInstance.close();
            }

            console.log(lastState);
            if (lastState) {
                $state.go(lastState);
            }

        }

    }


    function configStates($stateProvider, $urlRouterProvider) {
        $stateProvider.state('default', {
            url: '/',
            templateUrl: 'views/home.html',
            controller: 'HomeController as vm'
        });

        $urlRouterProvider.when('', '/');

    }

    function configHttpInterceptors($httpProvider) {
        $httpProvider.interceptors.push([
          '$injector',
          function ($injector) {
              return $injector.get('authInterceptor');
          }
        ]);

    }

    function AuthInterceptorFactory($rootScope, $q, AUTH_EVENTS) {
        return {
            request: function(config) {
                config.headers = config.headers || {};

                if ($rootScope.profile) {
                    config.headers.Authorization = 'Custom ' + $rootScope.profile.id;
                }

                return config;
            },
            responseError: function (response) { 
                $rootScope.$broadcast({
                    401: AUTH_EVENTS.notAuthenticated,
                    403: AUTH_EVENTS.notAuthorized,
                    419: AUTH_EVENTS.sessionTimeout,
                    440: AUTH_EVENTS.sessionTimeout
                }[response.status], response);
                return $q.reject(response);
            }
        };
    }

    function FormAutofillFixDirective($timeout) {
        return function (scope, element, attrs) {
            element.prop('method', 'post');
            if (attrs.ngSubmit) {
                $timeout(function () {
                    element
                      .unbind('submit')
                      .bind('submit', function (event) {
                          event.preventDefault();
                          element
                            .find('input, textarea, select')
                            .trigger('input')
                            .trigger('change')
                            .trigger('keydown');
                          scope.$apply(attrs.ngSubmit);
                      });
                });
            }
        };
    }

    function AuthServiceFactory($http, session) {
        var authService = {};
 
        authService.login = function (credentials) {
            return $http
              .post('api/authentication', credentials)
              .success(function (userSession) {
                  session.create(userSession.id, userSession.user.login, userSession.user.name);
                  return userSession;
              });
        };
 
        authService.isAuthenticated = function () {
            return !!session.userId;
        };

        return authService;
    }

    function LoginController($scope, $rootScope, AUTH_EVENTS, authService) {

        var vm = this;

        vm.error = false;
        vm.credentials = {
            username: '',
            password: ''
        };

        vm.login = function (credentials) {
            authService.login(credentials)
                .success(function (user) {
                    $rootScope.$broadcast(AUTH_EVENTS.loginSuccess);
                    $rootScope.setCurrentUser(user);
                })
                .error(function () {
                    vm.error = true;
                    $rootScope.$broadcast(AUTH_EVENTS.loginFailed);
                });
        };
    }

    function SessionService() {
        this.create = function (sessionId, userId, userName) {
            this.id = sessionId;
            this.userId = userId;
            this.userName = userName;
        };
        this.destroy = function () {
            this.id = null;
            this.userId = null;
            this.userName = null;
        };
    }

    function HomeController($uibModal, $http) {
        var vm = this;

        vm.editorInstance = null;
        vm.sql = '';
        vm.tabs = [];
        vm.columns = [];
        vm.rows = [];
        vm.newTab = newTab;
        vm.closeTab = closeTab;
        vm.executeCommand = executeCommand;

        init();

        function init() {
            
        }

        function getSelectedText(tab, editor) {
            var selectedText = editor.session.getTextRange(editor.getSelectionRange());

            if (!selectedText) {
                selectedText = tab.sql || '';
            }

            return selectedText;
        }


        function executeCommand(tab, editor) {

            var selectedText = getSelectedText(tab, editor);

            vm.rows.splice(0, vm.rows.length);
            vm.columns.splice(0, vm.columns.length);

            $http.post('api/sql', { commandText: selectedText, connection: tab.connection }).success(function (data) {

                if (!data || !data.length) {
                    return;
                }

                for (var i = 0; i < data.length; i++) {

                    var item = data[i];
                    vm.rows.push(item);

                    for (var prop in item) {
                        if (item.hasOwnProperty(prop) && vm.columns.indexOf(prop) < 0) {
                            vm.columns.push(prop);
                        }
                    }
                }
            });
        }

        function closeTab(tab) {
            var idx = vm.tabs.indexOf(tab);
            vm.tabs.splice(idx, 1);
        }

        function newTab() {
            var mi = $uibModal.open({
                templateUrl: 'views/connection-form.html',
                controller: 'NewConnectonController as vm'
            });

            mi.result.then(function (connection) {

                var tab = {
                    title: 'nuevo',
                    connection: connection,
                    aceOptions: {
                        mode: 'sql',
                        theme: 'monokai',
                        require: ['ace/ext/language_tools'],
                        advanced: {
                            enableBasicAutocompletion: true,
                            enableSnippets: true,
                            enableLiveAutocompletion: false
                        },
                        onLoad: function (editor) {
                            tab.editor = editor;
                            editor.commands.addCommand({
                                name: "executeCommand",
                                bindKey: { win: "Ctrl-Enter", mac: "Command-Enter" },
                                exec: function (_editor) {
                                    executeCommand(tab, _editor);
                                }
                            });
                        }
                    }
                };

                vm.tabs.push(tab);
            })

            
        }

    }

    function NewConnectonController($http, $uibModalInstance) {
        
        var vm = this;
        vm.ok = ok;

        vm.connection = { };
        vm.providers = [];

        init();

        function init() {
            $http.get('api/providers').success(function (facts) {
                vm.providers = facts;
            });
        }

        function ok() {
            vm.connection.providerName = vm.connection.provider.invariantName;
            $uibModalInstance.close(vm.connection);
        }
    }
    
})();