    <?php

    use Illuminate\Support\Facades\Route;
    use App\Http\Controllers\City2Controller;
    use App\Http\Controllers\MapController;
    use App\Http\Controllers\Admin\MapManagementController;
    use App\Http\Controllers\BusinessController;
    use App\Http\Controllers\JardinierController;
    use App\Http\Controllers\MagasinController;
    use App\Http\Controllers\PiscinisteController;
    use App\Http\Controllers\SearchController;
    use App\Http\Controllers\SearchJardinierController;
    use App\Http\Controllers\SearchMagasinController;
    use App\Http\Controllers\SearchPiscinisteController;

    Route::get('/city2', [City2Controller::class, 'index']);
    Route::get('/get-city-coordinates', [MapController::class, 'getCoordinates']);

    Route::get('/map', [MapController::class, 'showMap']);
    Route::post('/api/geocode', [MapController::class, 'geocode'])->name('geocode');
    Route::match(['GET', 'POST'], '/api/search', [MapController::class, 'search'])->name('search');
    Route::get('/api/search-by-location', [MapController::class, 'searchByLocation'])->name('search-by-location');

    Route::get('/map-management', [MapManagementController::class, 'index'])->name('admin.map-management');
    Route::post('/map-management/store', [MapManagementController::class, 'store'])->name('admin.map-management.store');
    Route::put('/map-management/{id}/update', [MapManagementController::class, 'update'])->name('admin.map-management.update');
    Route::delete('/map-management/{id}/delete', [MapManagementController::class, 'destroy'])->name('admin.map-management.delete');

    Route::post('/businesses/import', [BusinessController::class, 'import'])->name('businesses.import');
    Route::get('/businesses', [BusinessController::class, 'index'])->name('admin.businesses');      // Affiche la vue du dashboard
    Route::get('/businesses/data', [BusinessController::class, 'getBusinesses']);                  // Retourne les données JSON
    Route::post('/businesses', [BusinessController::class, 'store']);
    Route::get('/businesses/{id}', [BusinessController::class, 'show']);                           // Détails d'une entreprise
    Route::put('/businesses/{id}', [BusinessController::class, 'update']);                         // Mise à jour d'une entreprise
    Route::delete('/businesses/{id}', [BusinessController::class, 'destroy']);                     // Suppression d'une entreprise



    // Routes pour les piscinistes
    Route::get('/piscinistes', [PiscinisteController::class, 'index'])->name('piscinistes.index');
    Route::get('piscinistes-export', [PiscinisteController::class, 'exportCSV'])->name('piscinistes.export');
    Route::post('piscinistes-import', [PiscinisteController::class, 'importCSV'])->name('piscinistes.import');
    // Route::get('/piscinistes/map', [PiscinisteController::class, 'map'])->name('piscinistes.map');
    Route::get('/piscinistes/create', [PiscinisteController::class, 'create'])->name('piscinistes.create');
    Route::post('/piscinistes', [PiscinisteController::class, 'store'])->name('piscinistes.store');
    Route::get('/piscinistes/{id}', [PiscinisteController::class, 'show'])->name('piscinistes.show');
    Route::put('/piscinistes/{id}', [PiscinisteController::class, 'update'])->name('piscinistes.update');
    Route::get('/piscinistes/{id}/edit', [PiscinisteController::class, 'edit'])->name('piscinistes.edit');
    Route::delete('/piscinistes/{id}', [PiscinisteController::class, 'destroy'])->name('piscinistes.destroy');
    Route::get('/search-piscinistes', [SearchPiscinisteController::class, 'index'])->name('piscinistes.search');
    Route::post('/search-piscinistes', [SearchPiscinisteController::class, 'search'])->name('piscinistes.search.results');
    Route::get('/piscinistes/{id}/details', [SearchPiscinisteController::class, 'show'])->name('piscinistes.details');


    // Routes pour les magasins
    Route::get('/magasins', [MagasinController::class, 'index'])->name('magasins.index');
    Route::get('/magasins/create', [MagasinController::class, 'create'])->name('magasins.create');
    Route::post('/magasins', [MagasinController::class, 'store'])->name('magasins.store');
    Route::get('/magasins/{id}', [MagasinController::class, 'show'])->name('magasins.show');
    Route::get('/magasins/{id}/edit', [MagasinController::class, 'edit'])->name('magasins.edit');
    Route::put('/magasins/{id}', [MagasinController::class, 'update'])->name('magasins.update');
    Route::delete('/magasins/{id}', [MagasinController::class, 'destroy'])->name('magasins.destroy');
    Route::get('/magasins/export', [MagasinController::class, 'exportCSV'])->name('magasins.export');
    Route::post('/magasins/import', [MagasinController::class, 'importCSV'])->name('magasins.import');
    Route::get('/search-magasins', [SearchMagasinController::class, 'index'])->name('piscinistes.search');
   
    Route::get('/search-magasins',        [MagasinController::class, 'showSearchForm'])
        ->name('magasins.search');

    // Traitement AJAX de la recherche (POST)  
    Route::post('/search-mpstores',       [MagasinController::class, 'search'])
        ->name('mpstores.search');
    // Route::post('/search-magasins', [SearchMagasinController::class, 'search'])->name('piscinistes.search.result');
    // Route::post('/magasins/search', [MagasinController::class, 'search'])->name('magasins.search');
    // Route::get('/search-magasins', [MagasinController::class, 'search'])->name('magasins.search');


    Route::get('/search/map', [SearchController::class, 'map'])->name('search.map');

    // route des jardiniers
    Route::prefix('jardiniers')->group(function () {
        Route::get('/',              [JardinierController::class, 'index'])->name('jardiniers.index');
        Route::get('/create',        [JardinierController::class, 'create'])->name('jardiniers.create');
        Route::post('/',             [JardinierController::class, 'store'])->name('jardiniers.store');
        Route::get('/{id}',          [JardinierController::class, 'show'])->name('jardiniers.show');
        Route::get('/{id}/edit',     [JardinierController::class, 'edit'])->name('jardiniers.edit');
        Route::put('/{id}',          [JardinierController::class, 'update'])->name('jardiniers.update');
        Route::delete('/{id}',       [JardinierController::class, 'destroy'])->name('jardiniers.destroy');
        Route::get('/export',        [JardinierController::class, 'exportCSV'])->name('jardiniers.export');
        Route::post('/import',       [JardinierController::class, 'importCSV'])->name('jardiniers.import');
    });

    Route::prefix('search-jardiniers')->group(function () {
        Route::get('/',   [SearchJardinierController::class, 'index'])->name('jardiniers.search');
        Route::post('/',  [SearchJardinierController::class, 'search'])->name('jardiniers.search.results');
        Route::get('/{id}', [SearchJardinierController::class, 'show'])->name('jardiniers.details');
    });
