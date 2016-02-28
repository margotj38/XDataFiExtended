var app = angular.module('myApp', ['720kb.datepicker']);

app.config(function ($compileProvider) {
    $compileProvider.urlSanitizationWhitelist(/^\s*(https?|ftp|mailto|file|tel|blob):/);
})

app.config(['$httpProvider', function ($httpProvider) {
    $httpProvider.defaults.useXDomain = true;
    delete $httpProvider.defaults.headers.common['X-Requested-With'];
}]);







app.controller('TitleController', ['$scope', function ($scope) {
}]);
app.controller('MenuController', function ($scope) {
    $scope.menu = { s: true, e: false, i: false, x: false, res: "Asset" };
    $scope.vstock = function () { $scope.menu = { s: true, e: false, i: false, x: false, res: "Asset" } };
    $scope.vexchange = function () { $scope.menu = { s: false, e: true, i: false, x: false, res: "Currency" } };
    $scope.vinterest = function () { $scope.menu = { s: false, e: false, i: true, x: false, res: "Interest Rate" } };
    $scope.vxml = function () { $scope.menu = { s: false, e: false, i: false, x: true, res: "Import XML File" } };

});


app.controller('StockController', function ($scope, $http) {
    $scope.stock = "";
    $scope.options = ['Open', 'High', 'Low', 'Close', 'Volume'];
    $scope.selectedOptions = [];
    $scope.stringOptions = "";
    $scope.start = null;
    $scope.end = null;
    $scope.data = null;
    $scope.status = false;
    $scope.blob = null;
    $scope.url = null;
    $scope.erreurs = "";
    $scope.staterreurs = false;



    $scope.toggleSelection = function toggleSelection(option) {
        var idx = $scope.selectedOptions.indexOf(option);

        // is currently selected
        if (idx > -1) {
            $scope.selectedOptions.splice(idx, 1);
        }
            // is newly selected
        else {
            $scope.selectedOptions.push(option);
        }

    };

    $scope.optionsToString = function () {
        $scope.stringOptions = "";
        for (x in $scope.selectedOptions) {
            $scope.stringOptions += $scope.selectedOptions[x] + "~";
        }
        $scope.stringOptions = $scope.stringOptions.substring(0, $scope.stringOptions.length - 1);
        return $scope.stringOptions;
    }


    $scope.search = function () {
        $scope.status = false;
        $scope.erreurs = "";
        $scope.staterreurs = false;
        if ($scope.stock=="") {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucun actif en entrée.   ";
        }
        if ($scope.start == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune date de début.  ";
        }
        if ($scope.end == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune date de fin.  ";
        }
        if ($scope.end < $scope.start) {
            $scope.staterreurs = true;
            $scope.erreurs += "Date de début postérieure à la date de fin .  ";
        }
        if(!$scope.staterreurs) {
            $http.get('/actif/' + $scope.start + '/' + $scope.end + '/' + $scope.optionsToString() + '/' + $scope.stock)
            .success(function (data, status, headers, config) {
                $scope.data = data;
                $scope.status = true;
                $scope.blob = new Blob([JSON.stringify($scope.data)], { type: 'application/json' });
                $scope.url = (window.URL || window.webkitURL).createObjectURL($scope.blob);

            })
            .error(function (data, status, headers, config) {
                console.log(status);
            });
        }
        
    };

});

app.controller('ExchangeController', function ($scope, $http) {
    $scope.refCur = null;
    $scope.comparedCur = null;
    $scope.start = null;
    $scope.end = null;
    $scope.freq = null;
    $scope.data = null;
    $scope.status = false;
    $scope.compString = "";
    $scope.blob = null;
    $scope.url = null;
    $scope.erreurs = "";
    $scope.staterreurs = false;

    $scope.search = function () {
        $scope.status = false;
        $scope.erreurs = "";
        $scope.staterreurs = false;
        if ($scope.refCur == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune monnaie de référence en entrée.   ";
        }
        if ($scope.comparedCur == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune monnaie de comparaison en entrée.   ";
        }
        if ($scope.freq == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune fréquence en entrée.   ";
        }
        if ($scope.start == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune date de début.  ";
        }
        if ($scope.end == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune date de fin.  ";
        }
        if ($scope.end < $scope.start) {
            $scope.staterreurs = true;
            $scope.erreurs += "Date de début postérieure à la date de fin .  ";
        }
    if(!$scope.staterreurs) {
        $http.get('/exchange/' + $scope.start + '/' + $scope.end + '/' + $scope.freq + '/' + $scope.refCur + '/' + $scope.comparedCur)
            .success(function (data, status, headers, config) {
                $scope.data = data;
                $scope.compString = $scope.refCur + '/' + $scope.comparedCur;
                $scope.status = true;
                $scope.blob = new Blob([JSON.stringify($scope.data)], { type: 'application/json' });
                $scope.url = (window.URL || window.webkitURL).createObjectURL($scope.blob);
            })
            .error(function (data, status, headers, config) {
                console.log(status);
            });
    }
    };






});

app.controller('InterestController', function ($scope, $http) {
    $scope.name = null;
    $scope.start = null;
    $scope.end = null;
    $scope.data = null;
    $scope.status = false;
    $scope.statuseonia = false;
    $scope.blob = null;
    $scope.url = null;

    $scope.erreurs = "";
    $scope.staterreurs = false;

    $scope.search = function () {
        $scope.status = false;
        $scope.statuseonia = false;
        $scope.erreurs = "";
        $scope.staterreurs = false;
        if ($scope.name == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucun taux en entrée.   ";
        }
        if ($scope.start == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune date de début.  ";
        }
        if ($scope.end == null) {
            $scope.staterreurs = true;
            $scope.erreurs += "Aucune date de fin.  ";
        }
        if ($scope.end < $scope.start) {
            $scope.staterreurs = true;
            $scope.erreurs += "Date de début postérieure à la date de fin .  ";
        }
        if (!$scope.staterreurs) {
            $http.get('/interest/' + $scope.start + '/' + $scope.end + '/' + $scope.name)
                .success(function (data, status, headers, config) {
                    $scope.data = data;
                    if ($scope.name == "EONIA") {
                        $scope.statusnormal = false;
                        $scope.statuseonia = true;
                    } else {
                        $scope.statusnormal = true;
                    }
                    $scope.blob = new Blob([JSON.stringify($scope.data)], { type: 'application/json' });
                    $scope.url = (window.URL || window.webkitURL).createObjectURL($scope.blob);

                })
                .error(function (data, status, headers, config) {
                    console.log(status);
                });
        }
    };

});

app.controller('XmlController', function ($scope, $http) {
    $scope.test = "EN CONSTRUCTION ..."
});



